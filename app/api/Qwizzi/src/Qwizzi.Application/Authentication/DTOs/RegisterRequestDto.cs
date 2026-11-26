using System.ComponentModel.DataAnnotations;

namespace Qwizzi.Application.Authentication.DTOs;

public sealed class RegisterRequestDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    [MaxLength(50, ErrorMessage = "Username must be at most 50 characters long.")]
    [RegularExpression(
        @"^[A-Za-z0-9_-]+$",
        ErrorMessage =
            "Username may contain only letters, digits, underscores and hyphens.")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(255, ErrorMessage = "Email must be at most 255 characters long.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,64}$",
        ErrorMessage = "Password must be 8-64 characters and contain a lowercase letter, an uppercase letter, a digit and a special character.")]
    public string Password { get; init; } = string.Empty;

    [MaxLength(200)]
    public string Website { get; init; } = string.Empty;

    [MaxLength(2048)]
    public string TurnstileToken { get; init; } = string.Empty;
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
