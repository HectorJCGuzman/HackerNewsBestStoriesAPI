namespace HackerNewsBestStoriesAPI.Configuration
{
    public class HackerNewsApiSettings
    {
        public string BestStoriesUrl { get; set; } = string.Empty;
        public string ItemDetailsUrlFormat { get; set; } = string.Empty;
        public int MaxConcurrentRequests { get; set; } = 5;
        public int StoryIdsCacheMinutes { get; set; } = 5;
        public int StoryDetailsCacheMinutes { get; set; } = 60;
        public int FullResultCacheMinutes { get; set; } = 2;
    }
}