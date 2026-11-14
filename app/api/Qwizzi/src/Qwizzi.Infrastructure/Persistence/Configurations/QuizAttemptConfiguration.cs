using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("quiz_attempts");

        builder.HasKey(attempt => attempt.QuizAttemptId);

        builder.Property(attempt => attempt.QuizAttemptId)
            .HasColumnName("quiz_attempt_id");

        builder.Property(attempt => attempt.QuizId)
            .HasColumnName("quiz_id");

        builder.Property(attempt => attempt.UserId)
            .HasColumnName("user_id");

        builder.Property(attempt => attempt.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(attempt => attempt.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(attempt => attempt.Score)
            .HasColumnName("score")
            .IsRequired();

        builder.HasIndex(attempt => attempt.QuizId);
        builder.HasIndex(attempt => attempt.UserId);

        builder.HasOne(attempt => attempt.Quiz)
            .WithMany(quiz => quiz.Attempts)
            .HasForeignKey(attempt => attempt.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(attempt => attempt.User)
            .WithMany(user => user.QuizAttempts)
            .HasForeignKey(attempt => attempt.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(attempt => attempt.UserAnswers)
            .WithOne(userAnswer => userAnswer.QuizAttempt)
            .HasForeignKey(userAnswer => userAnswer.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}