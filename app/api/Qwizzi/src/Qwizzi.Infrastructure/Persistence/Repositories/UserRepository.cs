using Microsoft.EntityFrameworkCore;
using Qwizzi.Application.Authentication;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;

namespace Qwizzi.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.UserId == userId,
            cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Email.Trim().ToLower()
                    == email.Trim().ToLower(),
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(
            user => user.Email.Trim().ToLower()
                    == email.Trim().ToLower(),
            cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(
            user => user.Username.Trim().ToLower()
                    == username.Trim().ToLower(),
            cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task<bool> ExistsByEmailForOtherUserAsync(string email, Guid userId, CancellationToken cancellationToken = default) => dbContext.Users.AnyAsync(user => user.UserId != userId && user.Email.ToLower() == email.Trim().ToLower(), cancellationToken);
    public Task<bool> ExistsByUsernameForOtherUserAsync(string username, Guid userId, CancellationToken cancellationToken = default) => dbContext.Users.AnyAsync(user => user.UserId != userId && user.Username.ToLower() == username.Trim().ToLower(), cancellationToken);
    public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default) => dbContext.Users.CountAsync(user => user.Status && user.Role == UserRole.Administrator, cancellationToken);
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) => dbContext.Users.OrderBy(user => user.Username).ToListAsync(cancellationToken);
    public void Remove(User user) => dbContext.Users.Remove(user);
}
