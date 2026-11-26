using System.ComponentModel.DataAnnotations;

namespace Qwizzi.Application.Authentication.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;

    [MaxLength(200)]
    public string Website { get; init; } = string.Empty;

    [MaxLength(2048)]
    public string TurnstileToken { get; init; } = string.Empty;
}