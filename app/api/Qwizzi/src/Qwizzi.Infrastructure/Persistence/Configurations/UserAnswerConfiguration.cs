using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class UserAnswerConfiguration
    : IEntityTypeConfiguration<UserAnswer>
{
    public void Configure(EntityTypeBuilder<UserAnswer> builder)
    {
        builder.ToTable("user_answers");

        builder.HasKey(userAnswer => userAnswer.UserAnswerId);

        builder.Property(userAnswer => userAnswer.UserAnswerId)
            .HasColumnName("user_answer_id")
            .ValueGeneratedOnAdd();

        builder.Property(userAnswer => userAnswer.QuizAttemptId)
            .HasColumnName("quiz_attempt_id")
            .IsRequired();

        builder.Property(userAnswer => userAnswer.SelectedAnswerId)
            .HasColumnName("selected_answer_id")
            .IsRequired();

        builder.Property(userAnswer => userAnswer.IsCorrect)
            .HasColumnName("is_correct")
            .IsRequired();

        builder.HasIndex(userAnswer => userAnswer.QuizAttemptId);

        builder.HasIndex(userAnswer => userAnswer.SelectedAnswerId);

        builder.HasOne(userAnswer => userAnswer.QuizAttempt)
            .WithMany(attempt => attempt.UserAnswers)
            .HasForeignKey(userAnswer => userAnswer.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(userAnswer => userAnswer.SelectedAnswer)
            .WithMany()
            .HasForeignKey(userAnswer => userAnswer.SelectedAnswerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}