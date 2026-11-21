namespace Qwizzi.Application.Authentication.DTOs;
public sealed record AdminUserDto(Guid UserId, string Username, string Email, bool PasswordIsSet, string Role, bool Status, DateTime CreatedAt);
