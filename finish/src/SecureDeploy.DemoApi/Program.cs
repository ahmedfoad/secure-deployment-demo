using SecureDeploy.Shared;
using Serilog;

// ============================================================================
// TODO: Instructor Notes for Security Demo
// - This application demonstrates secure coding practices for CI/CD deployments
// - IMPORTANT: Never log sensitive data (tokens, passwords, PII)
// - See .github/workflows/ for GitHub Actions security gates
// - See Dockerfile for container hardening practices
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Configure structured logging with Serilog
// NOTE: Do NOT log request/response bodies in production if they contain secrets
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up - Secure Deployment Demo");

// ============================================================================
// HEALTH ENDPOINT: /health
// ============================================================================
app.MapHealthChecks("/health");

// ============================================================================
// DEMO ENDPOINT: /api/weatherforecast
// This endpoint demonstrates secure API development with logging
// ============================================================================
app.MapGet("/api/weatherforecast", GetWeatherForecast)
    .WithName("GetWeatherForecast")
    .WithOpenApi()
    .Produces<IEnumerable<WeatherForecast>>(StatusCodes.Status200OK);

// ============================================================================
// ROOT ENDPOINT: /
// ============================================================================
app.MapGet("/", GetWelcome)
    .WithName("GetWelcome")
    .WithOpenApi();

app.Run();

// Endpoint handlers
static IResult GetWeatherForecast(ILogger<Program> logger)
{
    logger.LogInformation("WeatherForecast endpoint called");
    
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        })
        .ToArray();
    
    var message = DemoMessageBuilder.BuildWeatherSummary(forecast.Length);
    logger.LogInformation("Returning {Count} weather forecasts. Message: {Message}", forecast.Length, message);
    
    return Results.Ok(forecast);
}

static IResult GetWelcome(ILogger<Program> logger, IHostEnvironment env)
{
    var message = DemoMessageBuilder.BuildWelcomeMessage(env.EnvironmentName);
    logger.LogInformation("Welcome message: {Message}", message);
    return Results.Json(new { message, timestamp = DateTime.UtcNow });
}
