using Qwizzi.Api.Authentication;

namespace Qwizzi.UnitTests;

public sealed class LoginChallengeTrackerTests
{
    [Fact]
    public void ChallengeIsRequiredAfterFailedAttemptAndResetAfterSuccess()
    {
        var tracker = new LoginChallengeTracker();

        Assert.False(tracker.IsChallengeRequired("user@example.com"));

        tracker.RecordFailure("user@example.com");

        Assert.True(tracker.IsChallengeRequired("USER@example.com"));

        tracker.Reset("user@example.com");

        Assert.False(tracker.IsChallengeRequired("user@example.com"));
    }

    [Fact]
    public void FailedAttemptsForDifferentEmailsAreTrackedSeparately()
    {
        var tracker = new LoginChallengeTracker();

        tracker.RecordFailure("first@example.com");

        Assert.True(tracker.IsChallengeRequired("first@example.com"));
        Assert.False(tracker.IsChallengeRequired("second@example.com"));
    }
}
