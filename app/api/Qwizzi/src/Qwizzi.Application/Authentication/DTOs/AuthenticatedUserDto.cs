namespace Qwizzi.Application.Authentication.DTOs;

public sealed record AuthenticatedUserDto(
    Guid UserId,
    string Username,
    string Email,
    string Role
    );