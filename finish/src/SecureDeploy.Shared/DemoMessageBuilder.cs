namespace SecureDeploy.Shared;

/// <summary>
/// Helper class for building demo messages.
/// Demonstrates shared library usage across projects.
/// </summary>
public static class DemoMessageBuilder
{
    /// <summary>
    /// Builds a welcome message for the demo API.
    /// </summary>
    public static string BuildWelcomeMessage(string environment = "Production")
    {
        return $"Welcome to Secure Deployment Demo API ({environment})";
    }

    /// <summary>
    /// Builds a formatted weather summary message.
    /// </summary>
    public static string BuildWeatherSummary(int count)
    {
        return $"Generated {count} weather forecast item(s) using hardened secure deployment practices.";
    }
}
