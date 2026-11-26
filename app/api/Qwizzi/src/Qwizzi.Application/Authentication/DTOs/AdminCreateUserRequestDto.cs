using System.ComponentModel.DataAnnotations;

namespace Qwizzi.Application.Authentication.DTOs;

public sealed class AdminCreateUserRequestDto
{
    [Required, MinLength(3), MaxLength(50), RegularExpression(@"^[A-Za-z0-9_-]+$")]
    public string Username { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,64}$")]
    public string TemporaryPassword { get; init; } = string.Empty;
}
