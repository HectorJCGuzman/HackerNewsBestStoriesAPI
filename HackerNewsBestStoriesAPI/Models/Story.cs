namespace HackerNewsBestStoriesAPI.Models
{
    // Model class representing a story from Hacker News with key details
    public class Story
    {
        public string Title { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int Score { get; set; }
        public int CommentCount { get; set; }
    }
}
