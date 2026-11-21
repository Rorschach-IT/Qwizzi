namespace Qwizzi.Application.Authentication;

public enum AuthError
{
    None = 0,
    InvalidRequest = 1,
    InvalidCredentials = 2,
    RegistrationConflict = 3
}

public sealed class DomainException : Exception
{
    public DomainError Error { get; }

    public DomainException(DomainError error)
    {
        Error = error;
    }
}

public sealed record DomainError(
    int StatusCode,
    string Title,
    string Detail);