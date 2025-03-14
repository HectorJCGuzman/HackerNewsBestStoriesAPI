namespace HackerNewsBestStoriesAPI.Extensions
{
    public static class DateTimeExtensions
    {
        // Extension method to convert a Unix timestamp to a UTC DateTime string in a specific format
        public static string UnixTimeStampToUtcDateTimeString(this long unixTimeStamp)
        {
            // Convert the Unix timestamp to DateTime in UTC
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).UtcDateTime;
            // Return the DateTime as a string in the specified format
            return dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }
    }
}