using Qwizzi.Application.Authentication.DTOs;

namespace Qwizzi.Application.Authentication;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResult> CreateByAdminAsync(
        AdminCreateUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> IsLoginChallengeRequiredAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUserDto?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}