using Qwizzi.Application.Quizzes.DTOs;

namespace Qwizzi.Application.Quizzes;

public interface IQuizService
{
    Task<IReadOnlyList<QuizSummaryDto>> BrowseAsync(string? search, int? categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuizSummaryDto>> GetOwnedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuizSummaryDto>> GetAllForModerationAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuizCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<QuizPlayDto?> GetForPlayingAsync(Guid quizId, CancellationToken cancellationToken = default);
    Task<QuizEditorDto?> GetForEditingAsync(Guid quizId, Guid userId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(Guid userId, QuizWriteRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid quizId, Guid userId, QuizWriteRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid quizId, Guid userId, CancellationToken cancellationToken = default);
    Task<StartQuizResult?> StartAsync(Guid quizId, Guid? userId, CancellationToken cancellationToken = default);
    Task<QuizAttemptResult?> SubmitAsync(Guid attemptId, Guid? userId, SubmitQuizRequest request, CancellationToken cancellationToken = default);
    Task<QuizStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ModerateAsync(Guid quizId, Guid moderatorId, QuizModerationRequest request, CancellationToken cancellationToken = default);
}
