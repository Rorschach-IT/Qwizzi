using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.UserId);

        builder.Property(user => user.UserId)
            .HasColumnName("user_id");

        builder.Property(user => user.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(user => user.Username)
            .IsUnique();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasColumnName("role")
            .IsRequired();
        
        builder.Property(user => user.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(user => user.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired();

        builder.Property(user => user.LockoutLevel)
            .HasColumnName("lockout_level")
            .IsRequired();

        builder.Property(user => user.LockoutUntil)
            .HasColumnName("lockout_until")
            .HasColumnType("timestamp with time zone");

        builder.Property(user => user.MustChangePassword)
            .HasColumnName("must_change_password")
            .IsRequired();

        builder.Property(user => user.TemporaryPasswordExpiresAt)
            .HasColumnName("temporary_password_expires_at")
            .HasColumnType("timestamp with time zone");

        builder.HasMany(user => user.CreatedQuizzes)
            .WithOne(quiz => quiz.Author)
            .HasForeignKey(quiz => quiz.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.QuizAttempts)
            .WithOne(attempt => attempt.User)
            .HasForeignKey(attempt => attempt.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}