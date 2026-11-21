using System.ComponentModel.DataAnnotations;
namespace Qwizzi.Application.Authentication.DTOs;
public sealed class ChangeEmailRequestDto { [Required, EmailAddress, MaxLength(255)] public string Email { get; init; } = string.Empty; }
