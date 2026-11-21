using System.ComponentModel.DataAnnotations;
namespace Qwizzi.Application.Authentication.DTOs;
public sealed class ChangeUsernameRequestDto { [Required, MinLength(3), MaxLength(50), RegularExpression(@"^[A-Za-z0-9_-]+$")] public string Username { get; init; } = string.Empty; }
