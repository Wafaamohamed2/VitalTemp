namespace VitalTemp.Infrastructure.Services;

/// <summary>
/// Centralized detection of whether an external API key is actually usable.
/// A key is considered NOT configured when it is null/empty, starts with "mock",
/// or is a bracketed placeholder such as "[ENCRYPTION_KEY]". This prevents the
/// FortyGuard and Gemini clients from firing doomed live HTTP requests (and the
/// resulting error logs) when only a placeholder is present in appsettings.
/// </summary>
public static class ApiKeyHelper
{
    public static bool IsConfigured(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        if (apiKey.StartsWith("mock", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (apiKey.StartsWith("[", StringComparison.OrdinalIgnoreCase) &&
            apiKey.EndsWith("]", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
