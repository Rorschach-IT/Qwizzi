using Qwizzi.Application.Abstractions.Persistence;
using Qwizzi.Application.Quizzes.DTOs;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;

namespace Qwizzi.Application.Quizzes;

public sealed class QuizService(IQuizRepository quizzes, IUnitOfWork unitOfWork) : IQuizService
{
    public async Task<IReadOnlyList<QuizSummaryDto>> BrowseAsync(string? search, int? categoryId, CancellationToken cancellationToken = default) =>
        (await quizzes.GetApprovedAsync(search?.Trim(), categoryId, cancellationToken)).Select(ToSummary).ToList();

    public async Task<IReadOnlyList<QuizSummaryDto>> GetOwnedAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await quizzes.GetOwnedAsync(userId, cancellationToken)).Select(ToSummary).ToList();

    public async Task<IReadOnlyList<QuizSummaryDto>> GetAllForModerationAsync(CancellationToken cancellationToken = default) =>
        (await quizzes.GetAllForModerationAsync(cancellationToken)).Select(ToSummary).ToList();

    public async Task<IReadOnlyList<QuizCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        (await quizzes.GetCategoriesAsync(cancellationToken)).Select(category => new QuizCategoryDto(category.QuizCategoryId, category.Name)).ToList();

    public async Task<QuizPlayDto?> GetForPlayingAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByIdAsync(quizId, cancellationToken);
        return quiz?.Status == QuizStatus.Approved ? ToPlay(quiz) : null;
    }

    public async Task<QuizEditorDto?> GetForEditingAsync(Guid quizId, Guid userId, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByIdAsync(quizId, cancellationToken);
        return quiz?.AuthorId == userId ? ToEditor(quiz) : null;
    }

    public async Task<Guid?> CreateAsync(Guid userId, QuizWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsValidAsync(request, userId, cancellationToken)) return null;
        var quiz = new Quiz
        {
            Title = request.Title.Trim(),
            CategoryId = request.CategoryId,
            Description = request.Description?.Trim(),
            CoverImageId = request.CoverImageId,
            AuthorId = userId,
            Status = QuizStatus.PendingVerification,
            Questions = request.Questions.Select(ToQuestion).ToList()
        };
        quizzes.Add(quiz);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return quiz.QuizId;
    }

    public async Task<bool> UpdateAsync(Guid quizId, Guid userId, QuizWriteRequest request, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByIdAsync(quizId, cancellationToken);
        if (quiz?.AuthorId != userId || !await IsValidAsync(request, userId, cancellationToken)) return false;
        if (await quizzes.HasAttemptsAsync(quizId, cancellationToken)) return false;

        quiz.Title = request.Title.Trim();
        quiz.CategoryId = request.CategoryId;
        quiz.Description = request.Description?.Trim();
        quiz.CoverImageId = request.CoverImageId;
        quiz.Status = QuizStatus.PendingVerification;
        quiz.VerifiedById = null;
        quiz.VerifiedAt = null;
        quiz.RejectionReason = null;
        quiz.UpdatedAt = DateTime.UtcNow;
        quiz.Questions.Clear();
        foreach (var question in request.Questions) quiz.Questions.Add(ToQuestion(question));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid quizId, Guid userId, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByIdAsync(quizId, cancellationToken);
        if (quiz?.AuthorId != userId) return false;
        quizzes.Remove(quiz);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<StartQuizResult?> StartAsync(Guid quizId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByIdAsync(quizId, cancellationToken);
        if (quiz?.Status != QuizStatus.Approved) return null;
        var attempt = new QuizAttempt { QuizId = quizId, UserId = userId };
        quizzes.Add(attempt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new StartQuizResult(attempt.QuizAttemptId, ToPlay(quiz), attempt.StartedAt);
    }

    public async Task<QuizAttemptResult?> SubmitAsync(Guid attemptId, Guid? userId, SubmitQuizRequest request, CancellationToken cancellationToken = default)
    {
        var attempt = await quizzes.GetAttemptAsync(attemptId, userId, cancellationToken);
        if (attempt is null || attempt.CompletedAt is not null || request.Answers is null) return null;
        var questionMap = attempt.Quiz.Questions.ToDictionary(question => question.QuestionId);
        if (request.Answers.Count != questionMap.Count
            || request.Answers.Keys.Any(questionId => !questionMap.ContainsKey(questionId)))
        {
            return null;
        }

        var results = new List<QuizQuestionResultDto>();
        foreach (var (questionId, answerId) in request.Answers)
        {
            var question = questionMap[questionId];
            var answer = question.Answers.SingleOrDefault(item => item.AnswerId == answerId);
            var correct = question.Answers.SingleOrDefault(item => item.IsCorrect);
            if (answer is null || correct is null) return null;
            results.Add(new QuizQuestionResultDto(questionId, answerId, answer.IsCorrect, correct.AnswerId));
            attempt.UserAnswers.Add(new UserAnswer { SelectedAnswerId = answerId, IsCorrect = answer.IsCorrect });
        }

        var completedAt = DateTime.UtcNow;
        var score = results.Count(result => result.IsCorrect);
        if (!await quizzes.CompleteAttemptAsync(attempt, score, completedAt, cancellationToken)) return null;
        return new QuizAttemptResult(attemptId, score, questionMap.Count, completedAt, results);
    }

    public async Task<QuizStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var attempts = await quizzes.GetAttemptsForUserAsync(userId, cancellationToken);
        var summaries = attempts.Select(attempt => new QuizAttemptSummaryDto(
            attempt.QuizAttemptId,
            attempt.QuizId,
            attempt.Quiz.Title,
            attempt.StartedAt,
            attempt.CompletedAt,
            attempt.Score,
            attempt.Quiz.Questions.Count)).ToList();
        var completed = attempts.Where(attempt => attempt.CompletedAt is not null).ToList();
        var percentages = completed.Select(attempt => attempt.Quiz.Questions.Count == 0 ? 0d : attempt.Score * 100d / attempt.Quiz.Questions.Count).ToList();
        return new QuizStatisticsDto(attempts.Count, completed.Count, percentages.Count == 0 ? 0 : percentages.Average(), percentages.Count == 0 ? 0 : percentages.Max(), summaries);
    }

    public async Task<bool> ModerateAsync(Guid quizId, Guid moderatorId, QuizModerationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Status is not ((int)QuizStatus.Approved) and not ((int)QuizStatus.Rejected)) return false;
        if (request.Status == (int)QuizStatus.Rejected
            && (string.IsNullOrWhiteSpace(request.RejectionReason) || request.RejectionReason.Length > 1000)) return false;
        var quiz = await quizzes.GetByIdAsync(quizId, cancellationToken);
        if (quiz is null) return false;
        quiz.Status = (QuizStatus)request.Status;
        quiz.VerifiedById = moderatorId;
        quiz.VerifiedAt = DateTime.UtcNow;
        quiz.RejectionReason = request.Status == (int)QuizStatus.Rejected ? request.RejectionReason!.Trim() : null;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> IsValidAsync(QuizWriteRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.Title)
            || request.Title.Trim().Length > 200
            || request.Description?.Length > 2000
            || request.Questions is null
            || request.Questions.Count == 0
            || !await quizzes.CategoryExistsAsync(request.CategoryId, cancellationToken))
        {
            return false;
        }

        var imageIds = request.Questions
            .SelectMany(question => question.Answers.Select(answer => answer.ImageId).Append(question.ImageId))
            .Append(request.CoverImageId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        foreach (var imageId in imageIds)
        {
            if (!await quizzes.ImageOwnedByAsync(imageId, userId, cancellationToken)) return false;
        }

        return request.Questions.All(question =>
            question is not null
            && question.Answers is not null
            && !question.Answers.Any(answer => answer is null)
            && !string.IsNullOrWhiteSpace(question.Text)
            && question.Text.Trim().Length <= 2000
            && (question.Explanation?.Length ?? 0) <= 2000
            && question.Answers.Count >= 2
            && question.Answers.Count(answer => answer.IsCorrect) == 1
            && question.Answers.All(answer =>
                (!string.IsNullOrWhiteSpace(answer.Text) != answer.ImageId.HasValue)
                && (answer.Text?.Trim().Length ?? 0) <= 1000));
    }

    private static Question ToQuestion(QuizQuestionRequest request) => new()
    {
        Text = request.Text.Trim(),
        ImageId = request.ImageId,
        Explanation = request.Explanation?.Trim(),
        Answers = request.Answers.Select(answer => new Answer
        {
            Text = string.IsNullOrWhiteSpace(answer.Text) ? null : answer.Text.Trim(),
            ImageId = answer.ImageId,
            IsCorrect = answer.IsCorrect
        }).ToList()
    };

    private static QuizSummaryDto ToSummary(Quiz quiz) => new(
        quiz.QuizId, quiz.Title, quiz.Description, quiz.Category.Name, quiz.CoverImageId, quiz.CoverImage?.StorageKey,
        quiz.Author.Username, quiz.Questions.Count, (int)quiz.Status);

    private static QuizPlayDto ToPlay(Quiz quiz) => new(
        quiz.QuizId, quiz.Title, quiz.Category.Name, quiz.CoverImageId, quiz.CoverImage?.StorageKey,
        quiz.Questions.Select(question => new QuizQuestionDto(
            question.QuestionId, question.Text, question.ImageId, question.Image?.StorageKey, null,
            question.Answers.Select(answer => new QuizAnswerDto(answer.AnswerId, answer.Text, answer.ImageId, answer.Image?.StorageKey, null)).ToList())).ToList());

    private static QuizEditorDto ToEditor(Quiz quiz) => new(
        quiz.QuizId, quiz.Title, quiz.Description, quiz.CategoryId, quiz.CoverImageId, (int)quiz.Status,
        quiz.Questions.Select(question => new QuizQuestionDto(
            question.QuestionId, question.Text, question.ImageId, question.Image?.StorageKey, question.Explanation,
            question.Answers.Select(answer => new QuizAnswerDto(answer.AnswerId, answer.Text, answer.ImageId, answer.Image?.StorageKey, answer.IsCorrect)).ToList())).ToList());
}
