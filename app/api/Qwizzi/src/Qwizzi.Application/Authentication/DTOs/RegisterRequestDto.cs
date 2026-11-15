using System.ComponentModel.DataAnnotations;

namespace Qwizzi.Application.Authentication.DTOs;

public sealed class RegisterRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(
        @"^[A-Za-z0-9_-]+$",
        ErrorMessage =
            "Username may contain only letters, digits, underscores and hyphens.")]
    public string Username { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 16, ErrorMessage = "Password must be at least 16 characters long.")]
    public string Password { get; init; } = string.Empty;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MinUniqueCharactersAttribute(int minimumUniqueCharacters) : ValidationAttribute
{
    public int MinimumUniqueCharacters { get; } = minimumUniqueCharacters;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || string.IsNullOrEmpty(password))
        {
            return ValidationResult.Success;
        }

        int uniqueCount = password.Distinct().Count();

        if (uniqueCount < MinimumUniqueCharacters)
        {
            return new ValidationResult(
                ErrorMessage ?? $"The field {validationContext.DisplayName} must contain at least {MinimumUniqueCharacters} unique characters.",
                [validationContext.MemberName!]);
        }

        return ValidationResult.Success;
    }
}