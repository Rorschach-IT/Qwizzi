using Qwizzi.Domain.Entities;

namespace Qwizzi.Domain.Security;

public static class AccountLockoutPolicy
{
    private const int AttemptsBeforeLockout = 10;

    public static DateTime? RecordFailedAttempt(User user, DateTime now)
    {
        if (user.LockoutUntil > now) return user.LockoutUntil;

        if (user.LockoutUntil is not null)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutUntil = null;
        }

        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts < AttemptsBeforeLockout) return null;

        user.FailedLoginAttempts = 0;
        user.LockoutLevel = Math.Min(user.LockoutLevel + 1, 3);
        var minutes = user.LockoutLevel switch
        {
            1 => 15,
            2 => 30,
            _ => 60
        };
        user.LockoutUntil = now.AddMinutes(minutes);
        return user.LockoutUntil;
    }

    public static void Reset(User user)
    {
        user.FailedLoginAttempts = 0;
        user.LockoutLevel = 0;
        user.LockoutUntil = null;
    }
}
