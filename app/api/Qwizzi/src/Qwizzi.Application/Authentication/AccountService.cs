using Qwizzi.Application.Abstractions.Persistence;
using Qwizzi.Application.Authentication.DTOs;
using Qwizzi.Domain.Enums;

namespace Qwizzi.Application.Authentication;

public sealed class AccountService(IUserRepository users, IPasswordService passwords, IUnitOfWork unitOfWork) : IAccountService
{
    public Task<AuthenticatedUserDto?> ChangeUsernameAsync(Guid userId, string username, CancellationToken cancellationToken = default) => ChangeUsernameCoreAsync(userId, username, cancellationToken);
    public Task<AuthenticatedUserDto?> ChangeUserUsernameAsync(Guid userId, string username, CancellationToken cancellationToken = default) => ChangeUsernameCoreAsync(userId, username, cancellationToken);
    public Task<AuthenticatedUserDto?> ChangeEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default) => ChangeEmailCoreAsync(userId, email, cancellationToken);
    public Task<AuthenticatedUserDto?> ChangeUserEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default) => ChangeEmailCoreAsync(userId, email, cancellationToken);

    public async Task<bool> ChangeOwnPasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null || passwords.Verify(user, currentPassword) == PasswordCheckResult.Failed) return false;
        user.PasswordHash = passwords.Hash(user, newPassword);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetPasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return false;
        user.PasswordHash = passwords.Hash(user, password);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteOwnAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return false;
        users.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<AdminUserDto>> GetUsersAsync(string sortBy, string sortOrder, CancellationToken cancellationToken = default)
    {
        var data = await users.GetAllAsync(cancellationToken);
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var ordered = sortBy.ToLowerInvariant() switch
        {
            "status" => desc ? data.OrderByDescending(x => x.Status) : data.OrderBy(x => x.Status),
            "createdat" or "created_at" => desc ? data.OrderByDescending(x => x.CreatedAt) : data.OrderBy(x => x.CreatedAt),
            _ => desc ? data.OrderByDescending(x => x.Username) : data.OrderBy(x => x.Username)
        };
        return ordered.Select(x => new AdminUserDto(x.UserId, x.Username, x.Email, !string.IsNullOrWhiteSpace(x.PasswordHash), x.Role.ToString(), x.Status, x.CreatedAt)).ToList();
    }

    public async Task<bool> SetStatusAsync(Guid actorId, Guid targetId, bool status, CancellationToken cancellationToken = default)
    {
        if (actorId == targetId) return false;
        var user = await users.GetByIdAsync(targetId, cancellationToken);
        if (user is null) return false;
        if (!status && user.Status && user.Role == UserRole.Administrator && await users.CountActiveAdministratorsAsync(cancellationToken) <= 1) return false;
        user.Status = status;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid actorId, Guid targetId, CancellationToken cancellationToken = default)
    {
        if (actorId == targetId) return false;
        var user = await users.GetByIdAsync(targetId, cancellationToken);
        if (user is null) return false;
        if (user.Status && user.Role == UserRole.Administrator && await users.CountActiveAdministratorsAsync(cancellationToken) <= 1) return false;
        users.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AuthenticatedUserDto?> ChangeUsernameCoreAsync(Guid id, string value, CancellationToken cancellationToken)
    {
        var username = value.Trim();
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null || await users.ExistsByUsernameForOtherUserAsync(username, id, cancellationToken)) return null;
        user.Username = username;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(user);
    }

    private async Task<AuthenticatedUserDto?> ChangeEmailCoreAsync(Guid id, string value, CancellationToken cancellationToken)
    {
        var email = value.Trim();
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null || await users.ExistsByEmailForOtherUserAsync(email, id, cancellationToken)) return null;
        user.Email = email;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(user);
    }

    private static AuthenticatedUserDto ToDto(Qwizzi.Domain.Entities.User user) => new(user.UserId, user.Username, user.Email, user.Role.ToString());
}
