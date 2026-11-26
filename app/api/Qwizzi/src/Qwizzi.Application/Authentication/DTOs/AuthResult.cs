namespace Qwizzi.Application.Authentication.DTOs;

public sealed record AuthResult(
    AuthenticatedUserDto? User,
    AuthError Error,
    Guid? TargetUserId = null,
    string? TargetUsername = null)
{
    public bool IsSuccess =>
        Error == AuthError.None && User is not null;

    public static AuthResult Success(AuthenticatedUserDto user)
    {
        return new AuthResult(user, AuthError.None);
    }

    public static AuthResult Failure(AuthError error, Guid? targetUserId = null, string? targetUsername = null)
    {
        return new AuthResult(null, error, targetUserId, targetUsername);
    }
}