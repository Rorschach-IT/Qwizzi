using System.ComponentModel.DataAnnotations;
namespace Qwizzi.Application.Authentication.DTOs;
public sealed class ChangePasswordRequestDto
{
    [Required] public string CurrentPassword { get; init; } = string.Empty;
    [Required, RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,64}$", ErrorMessage = "Password must be 8-64 characters and contain a lowercase letter, an uppercase letter, a digit and a special character.")] public string NewPassword { get; init; } = string.Empty;
    [Required, MaxLength(2048)] public string TurnstileToken { get; init; } = string.Empty;
}
