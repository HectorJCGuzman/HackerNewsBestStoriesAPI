using System.Text.Json.Serialization;
using HackerNewsBestStoriesAPI.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configure controllers with JSON serialization settings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camel case for property names in JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Ignore null properties when serializing
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add API documentation (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        // API documentation details
        Title = "Hacker News Best Stories API",  // API title
        Version = "v1",  // API version
        Description = "An API that retrieves the best stories from Hacker News",  // API description
        Contact = new OpenApiContact
        {
            Name = "Developer",  // Developer's name
            Email = "developer@example.com"  // Developer's contact email
        }
    });

    // This ensures that Swagger picks up the summary comments in the controllers
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "HackerNewsBestStoriesAPI.xml"));
});

// Configure logging (before other services)
builder.Logging.ClearProviders();  // Remove default loggers
builder.Logging.AddConsole();  // Add console logging
builder.Logging.AddDebug();  // Add debug logging

// Add Hacker News services (cache, API client, etc.)
builder.Services.AddHackerNewsServices(builder.Configuration);

// Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        // Allow all origins, methods, and headers
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add health checks to monitor the app's health status
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline

// Set up development environment-specific behavior
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Enable Swagger in development
    app.UseSwaggerUI();  // Enable Swagger UI
    app.UseDeveloperExceptionPage();  // Display detailed error pages in development
}
else
{
    // Set up production environment behavior
    app.UseExceptionHandler("/error");  // Use custom error handler in production
    app.UseHsts();  // Enforce HTTPS for secure connections
}

// Enable HTTPS redirection to ensure secure communication
app.UseHttpsRedirection();

// Apply the CORS policy that allows all origins
app.UseCors("AllowAll");

// Enable authorization middleware
app.UseAuthorization();

// Map endpoints (controllers and health checks)
app.MapControllers();  // Map controller endpoints
app.MapHealthChecks("/health");  // Map health check endpoint

// Run the application
app.Run();
