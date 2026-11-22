using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("answers", table =>
        {
            table.HasCheckConstraint(
                "ck_answers_exactly_one_text_or_image",
                "(NULLIF(BTRIM(text), '') IS NOT NULL) <> (image_id IS NOT NULL)");
        });

        builder.HasKey(answer => answer.AnswerId);

        builder.Property(answer => answer.AnswerId)
            .HasColumnName("answer_id")
            .ValueGeneratedOnAdd();

        builder.Property(answer => answer.QuestionId)
            .HasColumnName("question_id");

        builder.Property(answer => answer.Text)
            .HasColumnName("text")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(answer => answer.ImageId)
            .HasColumnName("image_id");

        builder.HasOne(answer => answer.Image)
            .WithMany()
            .HasForeignKey(answer => answer.ImageId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder.Property(answer => answer.IsCorrect)
            .HasColumnName("is_correct")
            .IsRequired();

        builder.HasIndex(answer => answer.QuestionId);

        builder.HasOne(answer => answer.Question)
            .WithMany(question => question.Answers)
            .HasForeignKey(answer => answer.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}