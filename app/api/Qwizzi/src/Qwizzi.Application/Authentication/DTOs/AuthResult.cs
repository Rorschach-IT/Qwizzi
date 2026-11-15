namespace Qwizzi.Application.Authentication.DTOs;

public sealed record AuthResult(
    AuthenticatedUserDto? User,
    AuthError Error)
{
    public bool IsSuccess =>
        Error == AuthError.None && User is not null;

    public static AuthResult Success(AuthenticatedUserDto user)
    {
        return new AuthResult(user, AuthError.None);
    }

    public static AuthResult Failure(AuthError error)
    {
        return new AuthResult(null, error);
    }
}