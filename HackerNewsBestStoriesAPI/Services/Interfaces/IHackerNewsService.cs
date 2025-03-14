using HackerNewsBestStoriesAPI.Models;
using System.Collections;

namespace HackerNewsBestStoriesAPI.Services.Interfaces
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count);
    }
}
