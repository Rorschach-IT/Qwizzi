using Qwizzi.Domain.Entities;

namespace Qwizzi.Application.Quizzes;

public interface IQuizRepository
{
    Task<List<Quiz>> GetApprovedAsync(string? search, int? categoryId, CancellationToken cancellationToken = default);
    Task<List<Quiz>> GetOwnedAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<List<Quiz>> GetAllForModerationAsync(CancellationToken cancellationToken = default);
    Task<List<QuizCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Quiz?> GetByIdAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> HasAttemptsAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<List<QuizAttempt>> GetAttemptsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, Guid? userId, CancellationToken cancellationToken = default);
    Task<bool> CompleteAttemptAsync(QuizAttempt attempt, int score, DateTime completedAt, CancellationToken cancellationToken = default);
    Task<bool> ImageOwnedByAsync(Guid imageId, Guid userId, CancellationToken cancellationToken = default);
    void Add(Quiz quiz);
    void Add(QuizAttempt attempt);
    void Remove(Quiz quiz);
}
