namespace Qwizzi.Domain.Entities;

public class ImageAsset
{
    public Guid ImageAssetId { get; set; } = Guid.NewGuid();

    public string StorageKey { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public Guid UploadedById { get; set; }

    public User UploadedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}