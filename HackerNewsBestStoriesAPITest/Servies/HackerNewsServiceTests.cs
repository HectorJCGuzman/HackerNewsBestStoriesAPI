using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HackerNewsBestStoriesAPI.Configuration;
using HackerNewsBestStoriesAPI.Models;
using HackerNewsBestStoriesAPI.Services;
using HackerNewsBestStoriesAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace HackerNewsBestStoriesAPITest.Servies
{
    public class HackerNewsServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<HackerNewsService>> _mockLogger;
        private readonly Mock<IOptions<HackerNewsApiSettings>> _mockSettings;
        private readonly HackerNewsApiSettings _settings;

        public HackerNewsServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<HackerNewsService>>();

            _settings = new HackerNewsApiSettings
            {
                BestStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json",
                ItemDetailsUrlFormat = "https://hacker-news.firebaseio.com/v0/item/{0}.json",
                MaxConcurrentRequests = 3,
                StoryIdsCacheMinutes = 5,
                StoryDetailsCacheMinutes = 60,
                FullResultCacheMinutes = 2
            };

            _mockSettings = new Mock<IOptions<HackerNewsApiSettings>>();
            _mockSettings.Setup(x => x.Value).Returns(_settings);
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsCachedResult_WhenCacheExists()
        {
            // Arrange
            var cachedStories = new List<Story>
            {
                new Story
                {
                    Title = "Test Story",
                    Uri = "https://example.com",
                    PostedBy = "testuser",
                    Time = "2023-01-01T12:00:00+00:00",
                    Score = 100,
                    CommentCount = 10
                }
            };

            _mockCacheService
                .Setup(x => x.GetFromCache<IEnumerable<Story>>("BestStories_5"))
                .Returns(cachedStories);

            var service = new HackerNewsService(
                _mockHttpClientFactory.Object,
                _mockCacheService.Object,
                _mockLogger.Object,
                _mockSettings.Object);

            // Act
            var result = await service.GetBestStoriesAsync(5);

            // Assert
            Assert.Equal(cachedStories, result);
            _mockHttpClientFactory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetBestStoriesAsync_FetchesStoryIdsAndDetails_WhenCacheDoesNotExist()
        {
            // Arrange            
            var storyIds = new int[] { 1 };
            var storyDetails = new HackerNewsItem
            {
                Id = 1,
                Title = "Test Story",
                Url = "https://example.com",
                By = "testuser",
                Time = 1609459200, // 2021-01-01 00:00:00 UTC
                Score = 100,
                Descendants = 10,
                Type = "story"
            };

            // Setup mock HTTP response for story IDs
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri == _settings.BestStoriesUrl),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(storyIds))
                });

            // Setup mock HTTP response for story details
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri.Contains("/item/")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(storyDetails))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient("HackerNewsApi")).Returns(httpClient);

            // Setup cache service to return null (cache miss)
            _mockCacheService
                .Setup(x => x.GetFromCache<IEnumerable<Story>>(It.IsAny<string>()))
                .Returns((IEnumerable<Story>)null);
            _mockCacheService
                .Setup(x => x.GetFromCache<int[]>("BestStories"))
                .Returns((int[])null);
            _mockCacheService
                .Setup(x => x.GetFromCache<HackerNewsItem>(It.IsAny<string>()))
                .Returns((HackerNewsItem)null);

            var service = new HackerNewsService(
                _mockHttpClientFactory.Object,
                _mockCacheService.Object,
                _mockLogger.Object,
                _mockSettings.Object);

            // Act
            var result = await service.GetBestStoriesAsync(1); // Pedimos solo 1 historia

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var story = result.First();
            Assert.Equal("Test Story", story.Title);
            Assert.Equal("https://example.com", story.Uri);
            Assert.Equal("testuser", story.PostedBy);
            Assert.Equal(100, story.Score);
            Assert.Equal(10, story.CommentCount);

            // Verify cache was set
            _mockCacheService.Verify(x =>
                x.SetCache(
                    It.Is<string>(s => s == "BestStories"),
                    It.Is<int[]>(arr => arr.SequenceEqual(storyIds)),
                    It.IsAny<TimeSpan>()
                ),
                Times.Once);

            _mockCacheService.Verify(x =>
                x.SetCache(
                    It.Is<string>(s => s.StartsWith("StoryDetails_")),
                    It.IsAny<HackerNewsItem>(),
                    It.IsAny<TimeSpan>()
                ),
                Times.Once);
        }

        [Fact]
        public async Task GetBestStoriesAsync_HandlesHttpExceptions_AndReturnsEmptyArray()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient("HackerNewsApi")).Returns(httpClient);

            // Setup cache service to return null (cache miss)
            _mockCacheService
                .Setup(x => x.GetFromCache<IEnumerable<Story>>(It.IsAny<string>()))
                .Returns((IEnumerable<Story>)null);
            _mockCacheService
                .Setup(x => x.GetFromCache<int[]>("BestStories"))
                .Returns((int[])null);

            var service = new HackerNewsService(
                _mockHttpClientFactory.Object,
                _mockCacheService.Object,
                _mockLogger.Object,
                _mockSettings.Object);

            // Act
            var result = await service.GetBestStoriesAsync(5);

            // Assert
            Assert.Empty(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}