using Qwizzi.Domain.Entities;

namespace Qwizzi.Application.Authentication;

public interface IPasswordService
{
    string Hash(User user, string password);

    PasswordCheckResult Verify(User user, string password);
}