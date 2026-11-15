using Microsoft.AspNetCore.Identity;
using Qwizzi.Application.Authentication;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Security;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    public string Hash(User user, string password)
    {
        return passwordHasher.HashPassword(user, password);
    }

    public PasswordCheckResult Verify(
        User user,
        string password)
    {
        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        return result switch
        {
            PasswordVerificationResult.Success =>
                PasswordCheckResult.Success,

            PasswordVerificationResult.SuccessRehashNeeded =>
                PasswordCheckResult.SuccessRehashNeeded,

            _ => PasswordCheckResult.Failed
        };
    }
}