using System.ComponentModel.DataAnnotations;
using Qwizzi.Application.Abstractions.Persistence;
using Qwizzi.Application.Authentication.DTOs;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;

namespace Qwizzi.Application.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return AuthResult.Failure(AuthError.HoneypotTriggered);
        }

        var cleanedRequest = new RegisterRequestDto
        {
            Username = request.Username?.Trim() ?? string.Empty,
            Email = request.Email?.Trim() ?? string.Empty,
            Password = request.Password ?? string.Empty,
            Website = request.Website ?? string.Empty
        };

        if (!IsValid(cleanedRequest))
        {
            return AuthResult.Failure(AuthError.InvalidRequest);
        }

        var emailExists = await _userRepository.ExistsByEmailAsync(
            cleanedRequest.Email,
            cancellationToken);

        if (emailExists)
        {
            return AuthResult.Failure(
                AuthError.RegistrationConflict);
        }

        var usernameExists =
            await _userRepository.ExistsByUsernameAsync(
                cleanedRequest.Username,
                cancellationToken);

        if (usernameExists)
        {
            return AuthResult.Failure(
                AuthError.RegistrationConflict);
        }

        var user = new User
        {
            Username = cleanedRequest.Username,
            Email = cleanedRequest.Email,
            Role = UserRole.User,
            Status = true
        };

        user.PasswordHash = _passwordService.Hash(
            user,
            cleanedRequest.Password);

        _userRepository.Add(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            return AuthResult.Failure(
                AuthError.RegistrationConflict);
        }

        return AuthResult.Success(ToDto(user));
    }

    public async Task<AuthResult> CreateByAdminAsync(
        AdminCreateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        if (username.Length is < 3 or > 50
            || email.Length > 255
            || !new EmailAddressAttribute().IsValid(email))
        {
            return AuthResult.Failure(AuthError.InvalidRequest);
        }

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken)
            || await _userRepository.ExistsByUsernameAsync(username, cancellationToken))
        {
            return AuthResult.Failure(AuthError.RegistrationConflict);
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Role = UserRole.User,
            Status = true,
            MustChangePassword = true,
            TemporaryPasswordExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        user.PasswordHash = _passwordService.Hash(user, request.TemporaryPassword);
        _userRepository.Add(user);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            return AuthResult.Failure(AuthError.RegistrationConflict);
        }

        return AuthResult.Success(ToDto(user));
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var cleanedRequest = new LoginRequestDto
        {
            Email = request.Email?.Trim() ?? string.Empty,
            Password = request.Password ?? string.Empty,
            Website = request.Website ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(cleanedRequest.Website))
        {
            return AuthResult.Failure(AuthError.HoneypotTriggered);
        }

        if (!IsValid(cleanedRequest))
        {
            return AuthResult.Failure(AuthError.InvalidRequest);
        }

        var user = await _userRepository.GetByEmailAsync(
            cleanedRequest.Email,
            cancellationToken);

        if (user is null)
        {
            return AuthResult.Failure(
                AuthError.InvalidCredentials);
        }

        var now = DateTime.UtcNow;
        if (user.LockoutUntil > now)
        {
            return AuthResult.Failure(AuthError.AccountLocked, user.UserId, user.Username);
        }

        var passwordResult = _passwordService.Verify(
            user,
            cleanedRequest.Password);

        if (!CanAuthenticate(user))
        {
            return AuthResult.Failure(
                AuthError.InvalidCredentials,
                user.UserId,
                user.Username);
        }

        if (passwordResult == PasswordCheckResult.Failed)
        {
            var attempt = await _userRepository.RecordFailedLoginAsync(user.UserId, now, cancellationToken);
            return AuthResult.Failure(attempt.LockoutUntil.HasValue ? AuthError.AccountLocked : AuthError.InvalidCredentials, user.UserId, user.Username);
        }

        if (!await _userRepository.CompleteSuccessfulLoginAsync(user.UserId, now, cancellationToken))
        {
            return AuthResult.Failure(AuthError.AccountLocked, user.UserId, user.Username);
        }

        if (user.TemporaryPasswordExpiresAt <= now)
        {
            user.MustChangePassword = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (passwordResult ==
            PasswordCheckResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordService.Hash(
                user,
                cleanedRequest.Password);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return AuthResult.Success(ToDto(user));
    }

    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null || !CanAuthenticate(user))
        {
            return null;
        }

        return ToDto(user);
    }

    public async Task<bool> IsLoginChallengeRequiredAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(
            email.Trim(),
            cancellationToken);
        return user is { FailedLoginAttempts: > 0 };
    }

    private static bool CanAuthenticate(User user)
    {
        return user.Status
            && user.Role is UserRole.User or UserRole.Administrator;
    }

    private static AuthenticatedUserDto ToDto(User user)
    {
        return new AuthenticatedUserDto(
            user.UserId,
            user.Username,
            user.Email,
            user.Role.ToString(),
            user.MustChangePassword);
    }

    private static bool IsValid(object request)
    {
        return Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults: null,
            validateAllProperties: true);
    }
}