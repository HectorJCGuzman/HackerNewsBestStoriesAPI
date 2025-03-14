# Hacker News Best Stories API

A high-performance ASP.NET Core RESTful API that retrieves the best stories from Hacker News based on their score. This project is built with .NET 8 and implements efficient caching and resilience patterns to handle large numbers of requests without overloading the Hacker News API.

## Features

- 📊 Retrieve the top N stories from Hacker News sorted by score
- 🚀 Efficient caching mechanism for optimal performance
- 🔄 Resilience patterns (retry, circuit breaker, timeout) for API stability
- 🔍 Swagger documentation for easy API exploration
- 🛡️ Rate limiting and concurrency control to prevent Hacker News API overload
- 🔬 Comprehensive test coverage

## API Overview

The API exposes a single endpoint that retrieves the best stories from Hacker News:

```
GET /api/stories/best?count={n}
```

Where `n` is the number of stories to return (default: 10, maximum: 500).

### Response Format

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  },
  // ... more stories
]
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (optional)

### Running the Application

1. Clone the repository:
   ```
   git clone https://github.com/HectorJCGuzman/HackerNewsBestStoriesAPI.git   
   ```
   ```   
   cd HackerNewsBestStoriesAPI
   ```

2. Build the solution:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   cd HackerNewsBestStoriesAPI #again
   ```
   ```   
   dotnet run
   ```

4. The API will be available at:
   - API: http://localhost:{port}/api/stories/best
   - Swagger UI: http://localhost:{port}/swagger
  
   The port will be visible in the terminal when you run the application on a line such as “ Now listening on: http://localhost:{port} ”

### Using Docker

Alternatively, you can run the application using Docker if you run the application from Visual Studio and have Docker Desktop installed.

## Configuration

The application can be configured through the `appsettings.json` file:

```json
"HackerNewsApi": {
  "BestStoriesUrl": "https://hacker-news.firebaseio.com/v0/beststories.json",
  "ItemDetailsUrlFormat": "https://hacker-news.firebaseio.com/v0/item/{0}.json",
  "MaxConcurrentRequests": 5,
  "StoryIdsCacheMinutes": 5,
  "StoryDetailsCacheMinutes": 60,
  "FullResultCacheMinutes": 2
}
```

Settings:
- `BestStoriesUrl`: URL to fetch the best stories IDs
- `ItemDetailsUrlFormat`: URL format to fetch details for a specific story
- `MaxConcurrentRequests`: Maximum number of concurrent HTTP requests to the Hacker News API
- `StoryIdsCacheMinutes`: Cache duration for the list of story IDs
- `StoryDetailsCacheMinutes`: Cache duration for individual story details
- `FullResultCacheMinutes`: Cache duration for the complete result set

## Architecture

The application follows a clean architecture approach with well-defined layers:

- **Controllers**: Handles HTTP requests and responses
- **Services**: Contains the business logic
- **Models**: Defines the data structures
- **Configuration**: Application settings
- **Extensions**: Helper methods and service configurations

### Caching Strategy

The application uses a multi-level caching strategy:

1. Story IDs are cached for 5 minutes (configurable)
2. Individual story details are cached for 60 minutes (configurable)
3. Complete result sets for specific counts are cached for 2 minutes (configurable)

This strategy significantly reduces the number of requests to the Hacker News API and improves response times.

### Resilience Patterns

The application implements several resilience patterns using Polly:

1. **Retry Policy**: Automatically retries failed HTTP requests with exponential backoff
2. **Circuit Breaker**: Prevents cascading failures by breaking the circuit after multiple failures
3. **Timeout Policy**: Limits the maximum duration of HTTP requests

## Running the Tests

To run the unit tests, from the main HackerNewsBestStoriesAPI folder:

```
cd HackerNewsBestStoriesAPITest
```
```
dotnet test
```

For running tests with coverage:

```
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Assumptions Made

1. The score of stories may change over time, so caching is used with appropriate expiration times.
2. The "best" stories are determined solely by their score, as specified in the requirements.
3. A maximum limit of 500 stories was implemented to prevent excessive load.

## Potential Enhancements

Given more time, the following enhancements could be made:

1. **WebSocket/SignalR for Real-Time Updates**  
   Implement **ASP.NET Core SignalR** to notify clients when new top stories are available without frequent polling.  
   - **Benefit**: Reduces API load and improves user experience with real-time updates.

2. **Distributed Caching with Redis**  
   Replace `MemoryCache` with **Redis** to share cached data across multiple API instances.  
   - **Benefit**: Enhances **scalability** and handles high traffic volumes efficiently.

3. **Optional Database Persistence**  
   Store top stories in a **database** (SQL or NoSQL) and update only when new stories appear.  
   - **Benefit**: **Reduces dependency** on Hacker News API and enables **historical analysis** of top stories.

4. **Message Queue Processing (RabbitMQ/Kafka)**  
   Offload story update requests to **RabbitMQ or Kafka**, instead of handling them synchronously.  
   - **Benefit**: Reduces **latency** and prevents API overload during peak request times.

5. **Advanced Filtering and Search**  
   Add support for **filtering stories by date, author, or keywords**.  
   - **Benefit**: Allows users to **customize** results based on their interests.

6. **Enhanced Observability with OpenTelemetry**  
   Integrate **OpenTelemetry** to capture advanced metrics and logs for performance monitoring.  
   - **Benefit**: Improves **debugging** and system optimization in production.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
