namespace Qwizzi.Domain.Entities;

public class UserAnswer
{
    public int UserAnswerId { get; set; }

    public Guid QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;
    
    public int SelectedAnswerId { get; set; }
    public Answer SelectedAnswer { get; set; } = null!;

    // Historical evaluation result recorded when the answer is submitted.
    public bool IsCorrect { get; set; }
}