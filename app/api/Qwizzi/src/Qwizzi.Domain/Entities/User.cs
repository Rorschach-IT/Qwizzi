using Qwizzi.Domain.Enums;

namespace Qwizzi.Domain.Entities;

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int FailedLoginAttempts { get; set; }
    public int LockoutLevel { get; set; }
    public DateTime? LockoutUntil { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? TemporaryPasswordExpiresAt { get; set; }
    
    // Relationship between
    public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}