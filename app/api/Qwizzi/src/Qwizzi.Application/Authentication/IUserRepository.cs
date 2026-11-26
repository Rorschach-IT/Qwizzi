using Qwizzi.Domain.Entities;

namespace Qwizzi.Application.Authentication;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailForOtherUserAsync(string email, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameForOtherUserAsync(string username, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LoginAttemptState> RecordFailedLoginAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default);
    Task<bool> CompleteSuccessfulLoginAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(Guid userId, string? expectedPasswordHash, string newPasswordHash, bool mustChangePassword, DateTime? temporaryPasswordExpiresAt, CancellationToken cancellationToken = default);
    Task<int> RequirePasswordChangeForAllAsync(CancellationToken cancellationToken = default);
    void Remove(User user);

    void Add(User user);
}
