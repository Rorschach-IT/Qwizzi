namespace Qwizzi.Api.Security;

public interface ITurnstileVerifier
{
    Task<TurnstileVerificationStatus> VerifyAsync(
        string token,
        string expectedAction,
        string expectedHostname,
        CancellationToken cancellationToken);
}

public enum TurnstileVerificationStatus
{
    Verified,
    Rejected,
    NotConfigured,
    Unavailable
}
