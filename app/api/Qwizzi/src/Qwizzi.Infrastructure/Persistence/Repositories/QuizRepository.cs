using Microsoft.EntityFrameworkCore;
using Qwizzi.Application.Quizzes;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;

namespace Qwizzi.Infrastructure.Persistence.Repositories;

public sealed class QuizRepository(AppDbContext dbContext) : IQuizRepository
{
    private IQueryable<Quiz> DetailedQuizzes() => dbContext.Quizzes
        .Include(quiz => quiz.Category)
        .Include(quiz => quiz.Author)
        .Include(quiz => quiz.CoverImage)
        .Include(quiz => quiz.Questions)
            .ThenInclude(question => question.Image)
        .Include(quiz => quiz.Questions)
            .ThenInclude(question => question.Answers)
                .ThenInclude(answer => answer.Image);

    public Task<List<Quiz>> GetApprovedAsync(string? search, int? categoryId, CancellationToken cancellationToken = default)
    {
        var query = DetailedQuizzes().Where(quiz => quiz.Status == QuizStatus.Approved);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(quiz => quiz.Title.ToLower().Contains(search.ToLower())
                || (quiz.Description != null && quiz.Description.ToLower().Contains(search.ToLower())));
        }
        if (categoryId.HasValue) query = query.Where(quiz => quiz.CategoryId == categoryId.Value);
        return query.OrderBy(quiz => quiz.Title).AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<List<Quiz>> GetOwnedAsync(Guid authorId, CancellationToken cancellationToken = default) =>
        DetailedQuizzes().Where(quiz => quiz.AuthorId == authorId).OrderByDescending(quiz => quiz.UpdatedAt ?? quiz.CreatedAt).AsNoTracking().ToListAsync(cancellationToken);

    public Task<List<Quiz>> GetAllForModerationAsync(CancellationToken cancellationToken = default) =>
        DetailedQuizzes().OrderByDescending(quiz => quiz.CreatedAt).AsNoTracking().ToListAsync(cancellationToken);

    public Task<List<QuizCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        dbContext.QuizCategories.AsNoTracking().OrderBy(category => category.Name).ToListAsync(cancellationToken);

    public Task<Quiz?> GetByIdAsync(Guid quizId, CancellationToken cancellationToken = default) =>
        DetailedQuizzes().SingleOrDefaultAsync(quiz => quiz.QuizId == quizId, cancellationToken);

    public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default) =>
        dbContext.QuizCategories.AnyAsync(category => category.QuizCategoryId == categoryId, cancellationToken);

    public Task<bool> HasAttemptsAsync(Guid quizId, CancellationToken cancellationToken = default) =>
        dbContext.QuizAttempts.AnyAsync(attempt => attempt.QuizId == quizId, cancellationToken);

    public Task<List<QuizAttempt>> GetAttemptsForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.QuizAttempts
            .Where(attempt => attempt.UserId == userId)
            .Include(attempt => attempt.Quiz)
                .ThenInclude(quiz => quiz.Questions)
            .OrderByDescending(attempt => attempt.StartedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, Guid? userId, CancellationToken cancellationToken = default) =>
        dbContext.QuizAttempts
            .Include(attempt => attempt.Quiz)
                .ThenInclude(quiz => quiz.Questions)
                    .ThenInclude(question => question.Answers)
            .SingleOrDefaultAsync(attempt => attempt.QuizAttemptId == attemptId && attempt.UserId == userId, cancellationToken);

    public async Task<bool> CompleteAttemptAsync(QuizAttempt attempt, int score, DateTime completedAt, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var current = await dbContext.QuizAttempts
            .FromSqlInterpolated($"SELECT * FROM quiz_attempts WHERE quiz_attempt_id = {attempt.QuizAttemptId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        if (current is null || current.CompletedAt is not null || current.UserId != attempt.UserId)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var tracked = dbContext.QuizAttempts.Local.SingleOrDefault(item => item.QuizAttemptId == attempt.QuizAttemptId);
        if (tracked is null)
        {
            dbContext.QuizAttempts.Attach(attempt);
            tracked = attempt;
        }
        else
        {
            dbContext.Entry(tracked).CurrentValues.SetValues(current);
        }

        tracked.Score = score;
        tracked.CompletedAt = completedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task<bool> ImageOwnedByAsync(Guid imageId, Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.ImageAssets.AnyAsync(image => image.ImageAssetId == imageId && image.UploadedById == userId, cancellationToken);

    public void Add(Quiz quiz) => dbContext.Quizzes.Add(quiz);
    public void Add(QuizAttempt attempt) => dbContext.QuizAttempts.Add(attempt);
    public void Remove(Quiz quiz) => dbContext.Quizzes.Remove(quiz);
}
