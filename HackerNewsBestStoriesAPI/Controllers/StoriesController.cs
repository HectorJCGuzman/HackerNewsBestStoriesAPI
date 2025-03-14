using HackerNewsBestStoriesAPI.Models;
using HackerNewsBestStoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HackerNewsBestStoriesAPI.Controllers
{
    // Controller responsible for handling API requests related to stories
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;  // Service for interacting with Hacker News API
        private readonly ILogger<StoriesController> _logger;  // Logger to log actions within the controller

        // Constructor to initialize dependencies
        public StoriesController(IHackerNewsService hackerNewsService, ILogger<StoriesController> logger)
        {
            _hackerNewsService = hackerNewsService ?? throw new ArgumentNullException(nameof(hackerNewsService));  // Ensure the service is provided
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is provided
        }

        /// <summary>
        /// Gets the best n stories from Hacker News by score.
        /// This endpoint retrieves the best stories from Hacker News, ordered by their score.
        /// You can specify how many stories you want by providing the count as a query parameter (default is 10 and the maximum is 500).
        /// </summary>
        /// <param name="count">Number of stories to return (default: 10)</param>
        /// <returns>An array of stories ordered by score descending</returns>
        /// <response code="200">Returns the best stories</response>
        /// <response code="400">If count is less than 1 or greater than 500</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("best")]
        [ProducesResponseType(typeof(IEnumerable<Story>), StatusCodes.Status200OK)]  // Success response type
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]  // BadRequest response type for invalid input
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]  // InternalServerError response type for unexpected errors
        public async Task<IActionResult> GetBestStories([FromQuery] int count = 10)
        {
            // Validate the count parameter to ensure it's within a valid range
            if (count < 1)
            {
                return BadRequest("Count must be greater than 0");  // Return 400 if count is invalid
            }

            if (count > 500)
            {
                return BadRequest("Count must be less than or equal to 500");  // Return 400 if count exceeds the upper limit
            }

            try
            {
                // Log the request to get the best stories
                _logger.LogInformation("Getting {Count} best stories", count);

                // Call the service to get the best stories asynchronously
                var stories = await _hackerNewsService.GetBestStoriesAsync(count);

                // Return the stories as a 200 OK response
                return Ok(stories);
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the operation
                _logger.LogError(ex, "Error getting best stories");

                // Return 500 InternalServerError response in case of an exception
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }
    }
}
