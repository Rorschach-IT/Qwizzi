using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(quiz => quiz.QuizId);

        builder.Property(quiz => quiz.QuizId)
            .HasColumnName("quiz_id");

        builder.Property(quiz => quiz.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(quiz => quiz.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();
        
        builder.Property(quiz => quiz.CoverImageId)
            .HasColumnName("cover_image_id");

        builder.HasOne(quiz => quiz.CoverImage)
            .WithMany()
            .HasForeignKey(quiz => quiz.CoverImageId)
            .OnDelete(DeleteBehavior.ClientNoAction);
        
        builder.Property(quiz => quiz.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.HasOne(quiz => quiz.Category)
            .WithMany(category => category.Quizzes)
            .HasForeignKey(quiz => quiz.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(quiz => quiz.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(quiz => quiz.AuthorId)
            .HasColumnName("author_id");

        builder.Property(quiz => quiz.VerifiedById)
            .HasColumnName("verified_by_id");

        builder.Property(quiz => quiz.VerifiedAt)
            .HasColumnName("verified_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(quiz => quiz.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(1000);

        builder.Property(quiz => quiz.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(quiz => quiz.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(quiz => quiz.Status);
        builder.HasIndex(quiz => quiz.AuthorId);

        builder.HasOne(quiz => quiz.Author)
            .WithMany(user => user.CreatedQuizzes)
            .HasForeignKey(quiz => quiz.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(quiz => quiz.VerifiedBy)
            .WithMany()
            .HasForeignKey(quiz => quiz.VerifiedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(quiz => quiz.Questions)
            .WithOne(question => question.Quiz)
            .HasForeignKey(question => question.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(quiz => quiz.Attempts)
            .WithOne(attempt => attempt.Quiz)
            .HasForeignKey(attempt => attempt.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}