using Microsoft.EntityFrameworkCore;
using Qwizzi.Application.Authentication;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;
using Qwizzi.Domain.Security;

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

    public async Task<LoginAttemptState> RecordFailedLoginAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var current = await LoadForUpdateAsync(userId, cancellationToken);
        if (current is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return new LoginAttemptState(false, null);
        }

        if (current.LockoutUntil > now)
        {
            await transaction.CommitAsync(cancellationToken);
            return new LoginAttemptState(false, current.LockoutUntil);
        }

        var user = AttachCurrent(current);
        var lockoutUntil = AccountLockoutPolicy.RecordFailedAttempt(user, now);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new LoginAttemptState(lockoutUntil is null, lockoutUntil);
    }

    public async Task<bool> CompleteSuccessfulLoginAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var current = await LoadForUpdateAsync(userId, cancellationToken);
        if (current is null || current.LockoutUntil > now)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var user = AttachCurrent(current);
        AccountLockoutPolicy.Reset(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task<int> RequirePasswordChangeForAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users
            .Where(user => user.Status)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(user => user.MustChangePassword, true),
                cancellationToken);

    public async Task<bool> UpdatePasswordAsync(Guid userId, string? expectedPasswordHash, string newPasswordHash, bool mustChangePassword, DateTime? temporaryPasswordExpiresAt, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var current = await LoadForUpdateAsync(userId, cancellationToken);
        if (current is null || (expectedPasswordHash is not null && current.PasswordHash != expectedPasswordHash))
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var user = AttachCurrent(current);
        user.PasswordHash = newPasswordHash;
        user.MustChangePassword = mustChangePassword;
        user.TemporaryPasswordExpiresAt = temporaryPasswordExpiresAt;
        AccountLockoutPolicy.Reset(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<User?> LoadForUpdateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var current = await dbContext.Users
            .FromSqlInterpolated($"SELECT * FROM users WHERE user_id = {userId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        return current;
    }

    private User AttachCurrent(User current)
    {
        var tracked = dbContext.Users.Local.SingleOrDefault(user => user.UserId == current.UserId);
        if (tracked is null)
        {
            dbContext.Users.Attach(current);
            return current;
        }

        dbContext.Entry(tracked).CurrentValues.SetValues(current);
        return tracked;
    }
}
