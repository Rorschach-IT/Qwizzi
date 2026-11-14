using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qwizzi.Domain.Entities;

namespace Qwizzi.Infrastructure.Persistence.Configurations;

public sealed class ImageAssetConfiguration
    : IEntityTypeConfiguration<ImageAsset>
{
    public void Configure(EntityTypeBuilder<ImageAsset> builder)
    {
        builder.ToTable("image_assets");

        builder.HasKey(image => image.ImageAssetId);

        builder.Property(image => image.ImageAssetId)
            .HasColumnName("image_asset_id")
            .ValueGeneratedNever();

        builder.Property(image => image.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(image => image.StorageKey)
            .IsUnique();

        builder.Property(image => image.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(image => image.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(image => image.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(image => image.Width)
            .HasColumnName("width")
            .IsRequired();

        builder.Property(image => image.Height)
            .HasColumnName("height")
            .IsRequired();

        builder.Property(image => image.UploadedById)
            .HasColumnName("uploaded_by_id")
            .IsRequired();

        builder.Property(image => image.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(image => image.UploadedBy)
            .WithMany()
            .HasForeignKey(image => image.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}