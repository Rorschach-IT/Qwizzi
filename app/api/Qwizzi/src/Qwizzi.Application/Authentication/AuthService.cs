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
        var cleanedRequest = new RegisterRequestDto
        {
            Username = request.Username?.Trim() ?? string.Empty,
            Email = request.Email?.Trim() ?? string.Empty,
            Password = request.Password ?? string.Empty
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

    public async Task<AuthResult> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var cleanedRequest = new LoginRequestDto
        {
            Email = request.Email?.Trim() ?? string.Empty,
            Password = request.Password ?? string.Empty
        };

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

        var passwordResult = _passwordService.Verify(
            user,
            cleanedRequest.Password);

        if (passwordResult == PasswordCheckResult.Failed
            || !CanAuthenticate(user))
        {
            return AuthResult.Failure(
                AuthError.InvalidCredentials);
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
            user.Role.ToString());
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