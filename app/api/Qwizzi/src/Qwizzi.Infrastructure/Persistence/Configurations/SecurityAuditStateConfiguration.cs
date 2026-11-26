using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class SecurityAuditStateConfiguration : IEntityTypeConfiguration<SecurityAuditState>
{
    public void Configure(EntityTypeBuilder<SecurityAuditState> builder)
    {
        builder.ToTable("security_audit_state");
        builder.HasKey(state => state.Id);
        builder.Property(state => state.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(state => state.LastSequence).HasColumnName("last_sequence").IsRequired();
        builder.Property(state => state.LastHash).HasColumnName("last_hash").HasMaxLength(64).IsRequired();
    }
}
