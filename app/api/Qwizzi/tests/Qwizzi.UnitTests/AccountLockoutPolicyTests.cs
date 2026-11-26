using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Security;

namespace Qwizzi.UnitTests;

public sealed class AccountLockoutPolicyTests
{
    [Fact]
    public void RepeatedSeriesIncreaseLockoutToSixtyMinutesAndThenStayCapped()
    {
        var user = new User();
        var now = new DateTime(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal(now.AddMinutes(15), CompleteFailureSeries(user, now));
        now = user.LockoutUntil!.Value;
        Assert.Equal(now.AddMinutes(30), CompleteFailureSeries(user, now));
        now = user.LockoutUntil!.Value;
        Assert.Equal(now.AddMinutes(60), CompleteFailureSeries(user, now));
        now = user.LockoutUntil!.Value;
        Assert.Equal(now.AddMinutes(60), CompleteFailureSeries(user, now));
    }

    [Fact]
    public void AttemptsDuringLockoutDoNotExtendItAndSuccessfulResetClearsTheLevel()
    {
        var user = new User { LockoutLevel = 1, FailedLoginAttempts = 3 };
        var now = new DateTime(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc);
        user.LockoutUntil = now.AddMinutes(15);

        Assert.Equal(user.LockoutUntil, AccountLockoutPolicy.RecordFailedAttempt(user, now));
        Assert.Equal(3, user.FailedLoginAttempts);

        AccountLockoutPolicy.Reset(user);

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Equal(0, user.LockoutLevel);
        Assert.Null(user.LockoutUntil);
    }

    private static DateTime? CompleteFailureSeries(User user, DateTime start)
    {
        for (var attempt = 1; attempt < 10; attempt++)
        {
            Assert.Null(AccountLockoutPolicy.RecordFailedAttempt(user, start));
        }

        return AccountLockoutPolicy.RecordFailedAttempt(user, start);
    }
}
