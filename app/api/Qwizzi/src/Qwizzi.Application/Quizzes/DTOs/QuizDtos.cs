namespace Qwizzi.Application.Quizzes.DTOs;

public sealed record QuizSummaryDto(Guid QuizId, string Title, string? Description, string Category, Guid? CoverImageId, string? CoverStorageKey, string Author, int QuestionCount, int Status);
public sealed record QuizCategoryDto(int CategoryId, string Name);
public sealed record QuizAnswerDto(int AnswerId, string? Text, Guid? ImageId, string? StorageKey, bool? IsCorrect);
public sealed record QuizQuestionDto(int QuestionId, string Text, Guid? ImageId, string? StorageKey, string? Explanation, IReadOnlyList<QuizAnswerDto> Answers);
public sealed record QuizPlayDto(Guid QuizId, string Title, string Category, Guid? CoverImageId, string? CoverStorageKey, IReadOnlyList<QuizQuestionDto> Questions);
public sealed record QuizEditorDto(Guid QuizId, string Title, string? Description, int CategoryId, Guid? CoverImageId, int Status, IReadOnlyList<QuizQuestionDto> Questions);
public sealed record QuizAnswerRequest(string? Text, Guid? ImageId, bool IsCorrect);
public sealed record QuizQuestionRequest(string Text, Guid? ImageId, string? Explanation, IReadOnlyList<QuizAnswerRequest> Answers);
public sealed record QuizWriteRequest(string Title, int CategoryId, string? Description, Guid? CoverImageId, IReadOnlyList<QuizQuestionRequest> Questions);
public sealed record StartQuizResult(Guid AttemptId, QuizPlayDto Quiz, DateTime StartedAt);
public sealed record SubmitQuizRequest(IReadOnlyDictionary<int, int> Answers);
public sealed record QuizAttemptResult(Guid AttemptId, int Score, int TotalQuestions, DateTime CompletedAt, IReadOnlyList<QuizQuestionResultDto> Questions);
public sealed record QuizQuestionResultDto(int QuestionId, int SelectedAnswerId, bool IsCorrect, int CorrectAnswerId);
public sealed record QuizAttemptSummaryDto(Guid AttemptId, Guid QuizId, string QuizTitle, DateTime StartedAt, DateTime? CompletedAt, int Score, int TotalQuestions);
public sealed record QuizStatisticsDto(int TotalAttempts, int CompletedAttempts, double AverageScorePercent, double BestScorePercent, IReadOnlyList<QuizAttemptSummaryDto> Attempts);
public sealed record QuizModerationRequest(int Status, string? RejectionReason);
