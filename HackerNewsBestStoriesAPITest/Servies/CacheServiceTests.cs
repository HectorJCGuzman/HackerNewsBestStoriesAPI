using System;
using HackerNewsBestStoriesAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HackerNewsBestStoriesAPITest.Servies
{
    public class CacheServiceTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<CacheService>> _mockLogger;

        public CacheServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<CacheService>>();
        }

        [Fact]
        public void GetFromCache_ReturnsCachedItem_WhenItemExists()
        {
            // Arrange
            var cacheService = new CacheService(_memoryCache, _mockLogger.Object);
            var testObject = new { Name = "Test" };
            var cacheKey = "test_key";

            // Add item to cache
            _memoryCache.Set(cacheKey, testObject, TimeSpan.FromMinutes(5));

            // Act
            var result = cacheService.GetFromCache<object>(cacheKey);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testObject, result);
        }

        [Fact]
        public void GetFromCache_ReturnsNull_WhenItemDoesNotExist()
        {
            // Arrange
            var cacheService = new CacheService(_memoryCache, _mockLogger.Object);
            var cacheKey = "nonexistent_key";

            // Act
            var result = cacheService.GetFromCache<object>(cacheKey);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SetCache_AddsItemToCache_WithExpirationTime()
        {
            // Arrange
            var cacheService = new CacheService(_memoryCache, _mockLogger.Object);
            var testObject = new { Name = "Test" };
            var cacheKey = "new_key";

            // Act
            cacheService.SetCache(cacheKey, testObject, TimeSpan.FromMinutes(5));

            // Assert
            var result = _memoryCache.Get<object>(cacheKey);
            Assert.NotNull(result);
            Assert.Equal(testObject, result);
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_WhenItemExists()
        {
            // Arrange
            var cacheService = new CacheService(_memoryCache, _mockLogger.Object);
            var testObject = new { Name = "Test" };
            var cacheKey = "try_get_key";

            // Add item to cache
            _memoryCache.Set(cacheKey, testObject, TimeSpan.FromMinutes(5));

            // Act
            var success = cacheService.TryGetValue<object>(cacheKey, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(testObject, result);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_WhenItemDoesNotExist()
        {
            // Arrange
            var cacheService = new CacheService(_memoryCache, _mockLogger.Object);
            var cacheKey = "nonexistent_key";

            // Act
            var success = cacheService.TryGetValue<object>(cacheKey, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}