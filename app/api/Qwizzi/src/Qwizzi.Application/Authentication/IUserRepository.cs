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
    void Remove(User user);

    void Add(User user);
}
