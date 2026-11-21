using System.ComponentModel.DataAnnotations;
namespace Qwizzi.Application.Authentication.DTOs;
public sealed class AdminSetPasswordRequestDto { [Required, RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,64}$", ErrorMessage = "Password must be 8-64 characters and contain a lowercase letter, an uppercase letter, a digit and a special character.")] public string Password { get; init; } = string.Empty; }
