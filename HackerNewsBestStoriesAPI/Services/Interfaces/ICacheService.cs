namespace HackerNewsBestStoriesAPI.Services.Interfaces
{
    public interface ICacheService
    {
        T? GetFromCache<T>(string key) where T : class;
        void SetCache<T>(string key, T value, TimeSpan expirationTime) where T : class;
        bool TryGetValue<T>(string key, out T? value) where T : class;
    }
}
