using HackerNewsBestStoriesAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsBestStoriesAPI.Services
{
    // Service class responsible for handling caching operations using in-memory cache
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;  // In-memory cache for storing items
        private readonly ILogger<CacheService> _logger;  // Logger to log cache hits, misses, and operations

        // Constructor to initialize the cache service with memory cache and logger
        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));  // Ensure memoryCache is provided
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is provided
        }

        // Attempts to retrieve an item from the cache by its key
        public T? GetFromCache<T>(string key) where T : class
        {
            // Check if the cache contains the item for the specified key
            if (_memoryCache.TryGetValue(key, out T? cachedItem))
            {
                // Log cache hit and return the cached item
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return cachedItem;
            }

            // Log cache miss if item is not found
            _logger.LogInformation("Cache miss for key: {Key}", key);
            return null;  // Return null if the item is not in the cache
        }

        // Adds an item to the cache with a specified expiration time
        public void SetCache<T>(string key, T value, TimeSpan expirationTime) where T : class
        {
            // Define cache entry options, including expiration time
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expirationTime);  // Set expiration time for the cached item

            // Store the item in the cache
            _memoryCache.Set(key, value, cacheEntryOptions);
            // Log the operation, including the key and expiration time
            _logger.LogInformation("Added to cache: {Key} with expiration of {ExpirationMinutes} minutes",
                key, expirationTime.TotalMinutes);
        }

        // Tries to retrieve an item from the cache, returning a boolean indicating success
        public bool TryGetValue<T>(string key, out T? value) where T : class
        {
            // Attempt to get the item from cache and return whether the operation was successful
            return _memoryCache.TryGetValue(key, out value);
        }
    }
}
