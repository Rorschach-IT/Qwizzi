using Qwizzi.Application.Authentication.DTOs;

namespace Qwizzi.Application.Authentication;

public interface IAccountService
{
    Task<AuthenticatedUserDto?> ChangeUsernameAsync(Guid userId, string username, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserDto?> ChangeEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    Task<bool> ChangeOwnPasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> SetPasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default);
    Task<bool> DeleteOwnAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<AdminUserDto>> GetUsersAsync(string sortBy, string sortOrder, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserDto?> ChangeUserUsernameAsync(Guid userId, string username, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserDto?> ChangeUserEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    Task<bool> SetStatusAsync(Guid actorId, Guid targetId, bool status, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid actorId, Guid targetId, CancellationToken cancellationToken = default);
}
