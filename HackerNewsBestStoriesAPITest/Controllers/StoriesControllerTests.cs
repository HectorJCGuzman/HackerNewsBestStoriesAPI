using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackerNewsBestStoriesAPI.Controllers;
using HackerNewsBestStoriesAPI.Models;
using HackerNewsBestStoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HackerNewsBestStoriesAPITest.Controllers
{
    public class StoriesControllerTests
    {
        private readonly Mock<IHackerNewsService> _mockHackerNewsService;
        private readonly Mock<ILogger<StoriesController>> _mockLogger;

        public StoriesControllerTests()
        {
            _mockHackerNewsService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<StoriesController>>();
        }

        [Fact]
        public async Task GetBestStories_ReturnsOkResult_WithStories()
        {
            // Arrange
            var stories = new List<Story>
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

            _mockHackerNewsService
                .Setup(service => service.GetBestStoriesAsync(5))
                .ReturnsAsync(stories);

            var controller = new StoriesController(_mockHackerNewsService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetBestStories(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Story>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetBestStories_ReturnsBadRequest_WhenCountIsLessThanOne(int count)
        {
            // Arrange
            var controller = new StoriesController(_mockHackerNewsService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetBestStories(count);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetBestStories_ReturnsBadRequest_WhenCountIsGreaterThan500()
        {
            // Arrange
            var controller = new StoriesController(_mockHackerNewsService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetBestStories(501);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetBestStories_ReturnsInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            _mockHackerNewsService
                .Setup(service => service.GetBestStoriesAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            var controller = new StoriesController(_mockHackerNewsService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetBestStories(10);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}