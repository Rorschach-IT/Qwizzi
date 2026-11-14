using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(question => question.QuestionId);

        builder.Property(question => question.QuestionId)
            .HasColumnName("question_id")
            .ValueGeneratedOnAdd();

        builder.Property(question => question.QuizId)
            .HasColumnName("quiz_id");

        builder.Property(question => question.Text)
            .HasColumnName("text")
            .HasMaxLength(2000)
            .IsRequired();
        
        builder.Property(question => question.ImageId)
            .HasColumnName("image_id");

        builder.HasOne(question => question.Image)
            .WithMany()
            .HasForeignKey(question => question.ImageId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder.Property(question => question.Explanation)
            .HasColumnName("explanation")
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(question => question.QuizId);

        builder.HasOne(question => question.Quiz)
            .WithMany(quiz => quiz.Questions)
            .HasForeignKey(question => question.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(question => question.Answers)
            .WithOne(answer => answer.Question)
            .HasForeignKey(answer => answer.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}