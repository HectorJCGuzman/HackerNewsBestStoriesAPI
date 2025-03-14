using System.Text.Json;
using HackerNewsBestStoriesAPI.Configuration;
using HackerNewsBestStoriesAPI.Extensions;
using HackerNewsBestStoriesAPI.Models;
using HackerNewsBestStoriesAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HackerNewsBestStoriesAPI.Services
{
    // Service class for interacting with the HackerNews API
    public class HackerNewsService : IHackerNewsService
    {
        private readonly IHttpClientFactory _httpClientFactory;  // Factory to create HTTP clients
        private readonly ICacheService _cacheService;  // Service to handle caching
        private readonly ILogger<HackerNewsService> _logger;  // Logger to log information and errors
        private readonly HackerNewsApiSettings _settings;  // Settings for the API and cache durations
        private const string BestStoriesCacheKey = "BestStories";  // Cache key for best stories
        private const string StoryDetailsCacheKeyPrefix = "StoryDetails_";  // Cache key prefix for story details

        // Constructor for initializing dependencies
        public HackerNewsService(
            IHttpClientFactory httpClientFactory,
            ICacheService cacheService,
            ILogger<HackerNewsService> logger,
            IOptions<HackerNewsApiSettings> settings)
        {
            // Dependency Injection with null checks for critical services
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        // Asynchronously retrieves the best stories from the Hacker News API
        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count)
        {
            try
            {
                // Cache key to store the result for the best stories based on count
                string fullResultCacheKey = $"{BestStoriesCacheKey}_{count}";

                // Attempt to fetch from cache first
                var cachedResult = _cacheService.GetFromCache<IEnumerable<Story>>(fullResultCacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Returning {Count} best stories from cache", count);
                    return cachedResult;
                }

                // Retrieve the list of story IDs for best stories
                var storyIds = await GetBestStoryIdsAsync();

                if (storyIds == null || !storyIds.Any())
                {
                    _logger.LogWarning("No story IDs returned from Hacker News API");
                    return Array.Empty<Story>();  // Return empty array if no IDs found
                }

                // Ensure the count does not exceed the number of available story IDs
                count = Math.Min(count, storyIds.Length);
                var selectedIds = storyIds.Take(count).ToArray();

                // Semaphore to limit the number of concurrent requests based on settings
                var semaphore = new SemaphoreSlim(_settings.MaxConcurrentRequests);

                // Create tasks to fetch details for each selected story ID
                var tasks = selectedIds.Select(async id =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await GetStoryDetailsAsync(id);  // Fetch story details asynchronously
                    }
                    finally
                    {
                        semaphore.Release();  // Release semaphore after processing the task
                    }
                });

                // Wait for all tasks to complete and get the results
                var stories = await Task.WhenAll(tasks);

                // Filter out any null values and order the results by score in descending order
                var result = stories.Where(s => s != null)
                                    .OrderByDescending(s => s!.Score)
                                    .Select(MapToStory)
                                    .ToList();

                // Cache the final result for future requests
                _cacheService.SetCache(fullResultCacheKey, result, TimeSpan.FromMinutes(_settings.FullResultCacheMinutes));

                return result;
            }
            catch (Exception ex)
            {
                // Log any errors and return an empty array in case of failure
                _logger.LogError(ex, "Error getting best stories");
                return Array.Empty<Story>();
            }
        }

        // Retrieves the list of best story IDs from the Hacker News API
        private async Task<int[]> GetBestStoryIdsAsync()
        {
            // Try to get story IDs from cache
            var cachedStoryIds = _cacheService.GetFromCache<int[]>(BestStoriesCacheKey);

            if (cachedStoryIds != null)
            {
                return cachedStoryIds;  // Return cached story IDs if available
            }

            // Fetch the story IDs from the Hacker News API if not in cache
            var client = _httpClientFactory.CreateClient("HackerNewsApi");
            var response = await client.GetAsync(_settings.BestStoriesUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var storyIds = JsonSerializer.Deserialize<int[]>(content) ?? Array.Empty<int>();

            // Cache the fetched story IDs
            _cacheService.SetCache(BestStoriesCacheKey, storyIds, TimeSpan.FromMinutes(_settings.StoryIdsCacheMinutes));

            return storyIds;
        }

        // Retrieves the details of a specific story by its ID
        private async Task<HackerNewsItem?> GetStoryDetailsAsync(int storyId)
        {
            // Generate the cache key for story details based on the story ID
            var cacheKey = $"{StoryDetailsCacheKeyPrefix}{storyId}";

            // Try to fetch story details from cache
            var cachedStory = _cacheService.GetFromCache<HackerNewsItem>(cacheKey);

            if (cachedStory != null)
            {
                return cachedStory;  // Return cached story if available
            }

            // Fetch the story details from the Hacker News API if not in cache
            var url = string.Format(_settings.ItemDetailsUrlFormat, storyId);

            try
            {
                var client = _httpClientFactory.CreateClient("HackerNewsApi");
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var story = JsonSerializer.Deserialize<HackerNewsItem>(content);

                if (story != null)
                {
                    // Cache the story details for future use
                    _cacheService.SetCache(cacheKey, story, TimeSpan.FromMinutes(_settings.StoryDetailsCacheMinutes));
                }

                return story;
            }
            catch (Exception ex)
            {
                // Log any errors and return null if story details cannot be fetched
                _logger.LogError(ex, "Error getting story details for ID: {StoryId}", storyId);
                return null;
            }
        }

        // Maps a HackerNewsItem to a Story object
        private static Story MapToStory(HackerNewsItem? item)
        {
            return item == null
                ? new Story()  // Return an empty Story object if the item is null
                : new Story
                {
                    Title = item.Title,
                    Uri = item.Url,
                    PostedBy = item.By,
                    Time = item.Time.UnixTimeStampToUtcDateTimeString(),
                    Score = item.Score,
                    CommentCount = item.Descendants
                };
        }
    }
}
