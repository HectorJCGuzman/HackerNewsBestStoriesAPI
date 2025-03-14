using HackerNewsBestStoriesAPI.Extensions;
using Xunit;

namespace HackerNewsBestStoriesAPITest.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Theory]
        [InlineData(1609459200, "2021-01-01T00:00:00+00:00")] // 2021-01-01 00:00:00 UTC
        [InlineData(1617235200, "2021-04-01T00:00:00+00:00")] // 2021-04-01 00:00:00 UTC
        [InlineData(1625097600, "2021-07-01T00:00:00+00:00")] // 2021-07-01 00:00:00 UTC
        public void UnixTimeStampToUtcDateTimeString_ConvertsCorrectly(long unixTimeStamp, string expected)
        {
            // Act
            var result = unixTimeStamp.UnixTimeStampToUtcDateTimeString();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}