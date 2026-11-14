namespace Qwizzi.Domain.Entities;

public class QuizAttempt
{
    public Guid QuizAttemptId { get; set; } = Guid.NewGuid();

    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    // Nulle dla ubiegających się gości bez logowania
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int Score { get; set; }

    // Relacje z odpowiedziami udzielonymi w trakcie podejścia
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}