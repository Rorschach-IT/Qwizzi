using System.ComponentModel.DataAnnotations;

namespace Qwizzi.Application.Authentication.DTOs;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;
}