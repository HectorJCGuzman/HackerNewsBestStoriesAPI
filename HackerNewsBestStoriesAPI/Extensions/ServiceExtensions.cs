using HackerNewsBestStoriesAPI.Configuration;
using HackerNewsBestStoriesAPI.Services.Interfaces;
using HackerNewsBestStoriesAPI.Services;
using Polly;

namespace HackerNewsBestStoriesAPI.Extensions
{
    // Extension class for configuring services related to Hacker News API
    public static class ServiceExtensions
    {
        // Extension method to register services for HackerNews in the IServiceCollection
        public static IServiceCollection AddHackerNewsServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the settings for HackerNewsApi from the configuration file
            services.Configure<HackerNewsApiSettings>(configuration.GetSection("HackerNewsApi"));

            // Register the in-memory cache service to be used in the application
            services.AddMemoryCache();

            // Register the CacheService implementation of ICacheService
            services.AddScoped<ICacheService, CacheService>();

            // Register an HTTP client for making requests to HackerNews API, with resilience policies
            services.AddHttpClient("HackerNewsApi", client =>
            {
                // Configure the base settings for the HttpClient using the configuration
                var settings = configuration.GetSection("HackerNewsApi").Get<HackerNewsApiSettings>();
            })
            // Apply a retry policy for handling transient errors
            .AddPolicyHandler(GetRetryPolicy())
            // Apply a circuit breaker policy to handle repeated failures
            .AddPolicyHandler(GetCircuitBreakerPolicy())
            // Apply a timeout policy to limit the maximum time for HTTP requests
            .AddPolicyHandler(GetTimeoutPolicy());

            // Register the HackerNewsService implementation of IHackerNewsService
            services.AddScoped<IHackerNewsService, HackerNewsService>();

            // Return the IServiceCollection to allow method chaining
            return services;
        }

        // Define the retry policy to handle transient HTTP errors
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy<HttpResponseMessage>
                // Handle specific exception types (HttpRequestException)
                .Handle<HttpRequestException>()
                // Or handle HTTP status code 429 (TooManyRequests)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                // Define the retry policy to retry up to 3 times with an exponential backoff
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        // Define the circuit breaker policy to stop retrying after a certain number of failures
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy<HttpResponseMessage>
                // Handle HTTP request exceptions
                .Handle<HttpRequestException>()
                // Open the circuit after 5 consecutive failures within 30 seconds
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }

        // Define the timeout policy to limit the maximum duration of HTTP requests
        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));  // Timeout after 10 seconds
        }
    }
}
