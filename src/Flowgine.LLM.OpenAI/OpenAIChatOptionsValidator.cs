using Microsoft.Extensions.Options;

namespace Flowgine.LLM.OpenAI;

/// <summary>
/// Validates <see cref="OpenAIChatOptions"/> configuration.
/// </summary>
public class OpenAIChatOptionsValidator : IValidateOptions<OpenAIChatOptions>
{
    /// <summary>
    /// Validates the specified options instance.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, OpenAIChatOptions options)
    {
        var failures = new List<string>();

        // Validate API Key
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            failures.Add("ApiKey is required and cannot be empty.");
        }

        // Validate BaseUrl format
        if (!string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri))
            {
                failures.Add("BaseUrl must be a valid absolute URI.");
            }
            else if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                failures.Add("BaseUrl must use http or https scheme.");
            }
        }

        // Validate DefaultModel
        if (string.IsNullOrWhiteSpace(options.DefaultModel))
        {
            failures.Add("DefaultModel cannot be empty.");
        }

        // Validate DefaultTemperature
        if (options.DefaultTemperature is < 0f or > 2f)
        {
            failures.Add("DefaultTemperature must be between 0.0 and 2.0 for OpenAI.");
        }

        // Validate DefaultMaxTokens
        if (options.DefaultMaxTokens is <= 0)
        {
            failures.Add("DefaultMaxTokens must be a positive number.");
        }

        // Validate RequestTimeout
        if (options.RequestTimeout is not null && options.RequestTimeout.Value <= TimeSpan.Zero)
        {
            failures.Add("RequestTimeout must be greater than zero.");
        }

        // Validate MaxRetries
        if (options.MaxRetries < 0)
        {
            failures.Add("MaxRetries cannot be negative.");
        }

        // Validate RetryDelay
        if (options.RetryDelay is not null && options.RetryDelay.Value < TimeSpan.Zero)
        {
            failures.Add("RetryDelay cannot be negative.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

