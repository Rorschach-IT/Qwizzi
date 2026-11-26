using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.ToTable("security_events");
        builder.HasKey(entry => entry.Sequence);
        builder.Property(entry => entry.Sequence).HasColumnName("sequence").ValueGeneratedNever();
        builder.Property(entry => entry.UserId).HasColumnName("user_id");
        builder.Property(entry => entry.Username).HasColumnName("username").HasMaxLength(50);
        builder.Property(entry => entry.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(entry => entry.IpAddress).HasColumnName("ip_address").HasMaxLength(64);
        builder.Property(entry => entry.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
        builder.Property(entry => entry.Succeeded).HasColumnName("succeeded").IsRequired();
        builder.Property(entry => entry.PreviousHash).HasColumnName("previous_hash").HasMaxLength(64).IsRequired();
        builder.Property(entry => entry.EntryHash).HasColumnName("entry_hash").HasMaxLength(64).IsRequired();
    }
}
