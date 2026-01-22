using SecureDeploy.Shared;
using Xunit;

namespace SecureDeploy.DemoApi.Tests;

/// <summary>
/// Unit tests for the Secure Deployment Demo API.
/// Tests verify that core functionality and models work correctly.
/// 
/// Note: For full integration tests (testing HTTP endpoints directly),
/// use WebApplicationFactory in a separate integration test project.
/// These tests focus on business logic and shared library components.
/// </summary>
public class DemoMessageBuilderTests
{
    // ========================================================================
    // TEST: Welcome Message Builder
    // Verifies that the DemoMessageBuilder creates expected messages
    // ========================================================================
    [Fact]
    public void BuildWelcomeMessage_WithDefaultEnvironment_ReturnsExpectedMessage()
    {
        // Arrange
        var expectedEnvironment = "Production";

        // Act
        var message = DemoMessageBuilder.BuildWelcomeMessage(expectedEnvironment);

        // Assert
        Assert.NotNull(message);
        Assert.Contains("Welcome to Secure Deployment Demo API", message);
        Assert.Contains("Production", message);
    }

    // ========================================================================
    // TEST: Welcome Message with Different Environment
    // Verifies that different environments are correctly included
    // ========================================================================
    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void BuildWelcomeMessage_WithVariousEnvironments_IncludesEnvironmentName(string environment)
    {
        // Act
        var message = DemoMessageBuilder.BuildWelcomeMessage(environment);

        // Assert
        Assert.Contains(environment, message);
    }

    // ========================================================================
    // TEST: Weather Summary Builder
    // Verifies that weather summary includes count
    // ========================================================================
    [Fact]
    public void BuildWeatherSummary_WithForecastCount_IncludesCount()
    {
        // Arrange
        var count = 5;

        // Act
        var message = DemoMessageBuilder.BuildWeatherSummary(count);

        // Assert
        Assert.NotNull(message);
        Assert.Contains("5", message);
        Assert.Contains("forecast", message.ToLower());
    }

    // ========================================================================
    // TEST: Weather Forecast Model
    // Verifies that WeatherForecast model calculates Fahrenheit correctly
    // ========================================================================
    [Fact]
    public void WeatherForecast_TemperatureConversion_CalculatesCorrectly()
    {
        // Arrange
        var forecast = new WeatherForecast
        {
            TemperatureC = 0,
            Date = DateTime.Now,
            Summary = "Freezing"
        };

        // Act
        var fahrenheit = forecast.TemperatureF;

        // Assert
        Assert.Equal(32, fahrenheit); // 0°C = 32°F
    }

    // ========================================================================
    // TEST: Weather Forecast Model with Different Temperature
    // Verifies temperature conversion for various values
    // ========================================================================
    [Theory]
    [InlineData(0, 32)]
    [InlineData(10, 50)]
    [InlineData(25, 77)]
    [InlineData(-10, 14)]
    public void WeatherForecast_VariousTemperatures_CalculatesFahrenheitCorrectly(int celsius, int expectedFahrenheit)
    {
        // Arrange
        var forecast = new WeatherForecast
        {
            TemperatureC = celsius,
            Date = DateTime.Now,
            Summary = "Test"
        };

        // Act
        var fahrenheit = forecast.TemperatureF;

        // Assert
        // Using approximate equality due to rounding in the formula
        Assert.InRange(fahrenheit, expectedFahrenheit - 1, expectedFahrenheit + 1);
    }
}

