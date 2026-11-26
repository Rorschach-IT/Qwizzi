using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Qwizzi.Api.Security;

public sealed class CloudflareTurnstileVerifier(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CloudflareTurnstileVerifier> logger) : ITurnstileVerifier
{
    private const string VerificationUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<TurnstileVerificationStatus> VerifyAsync(
        string token,
        string expectedAction,
        string expectedHostname,
        CancellationToken cancellationToken)
    {
        var secret = configuration["DOTNET_TURNSTILE_SECRET_KEY"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            logger.LogError("Cloudflare Turnstile is required but DOTNET_TURNSTILE_SECRET_KEY is not configured.");
            return TurnstileVerificationStatus.NotConfigured;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return TurnstileVerificationStatus.Rejected;
        }

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = secret,
            ["response"] = token
        });

        try
        {
            using var response = await httpClient.PostAsync(
                VerificationUrl,
                content,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Cloudflare Turnstile verification returned HTTP {StatusCode}.", (int)response.StatusCode);
                return TurnstileVerificationStatus.Unavailable;
            }

            var result = await response.Content.ReadFromJsonAsync<SiteVerifyResponse>(
                cancellationToken: cancellationToken);
            return result is { Success: true }
                   && string.Equals(result.Action, expectedAction, StringComparison.Ordinal)
                   && string.Equals(result.Hostname, expectedHostname, StringComparison.OrdinalIgnoreCase)
                ? TurnstileVerificationStatus.Verified
                : TurnstileVerificationStatus.Rejected;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Cloudflare Turnstile verification request failed.");
            return TurnstileVerificationStatus.Unavailable;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Cloudflare Turnstile verification timed out.");
            return TurnstileVerificationStatus.Unavailable;
        }
        catch (System.Text.Json.JsonException exception)
        {
            logger.LogWarning(exception, "Cloudflare Turnstile returned an invalid verification response.");
            return TurnstileVerificationStatus.Unavailable;
        }
    }

    private sealed record SiteVerifyResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("hostname")] string? Hostname,
        [property: JsonPropertyName("action")] string? Action);
}
