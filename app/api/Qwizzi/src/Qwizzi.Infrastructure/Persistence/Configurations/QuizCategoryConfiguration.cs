using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class QuizCategoryConfiguration
    : IEntityTypeConfiguration<QuizCategory>
{
    public void Configure(EntityTypeBuilder<QuizCategory> builder)
    {
        builder.ToTable("quiz_categories");

        builder.HasKey(category => category.QuizCategoryId);

        builder.Property(category => category.QuizCategoryId)
            .HasColumnName("quiz_category_id")
            .ValueGeneratedOnAdd();

        builder.Property(category => category.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(category => category.Name)
            .IsUnique();
    }
}