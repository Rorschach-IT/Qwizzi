namespace Qwizzi.Application.Images;

public interface IImageAssetService
{
    Task<ImageAssetDto?> UploadAsync(Guid userId, string fileName, string contentType, long length, Stream content, CancellationToken cancellationToken = default);
    Task<ImageFileDto?> GetAsync(Guid imageId, CancellationToken cancellationToken = default);
}

public sealed record ImageAssetDto(Guid ImageAssetId, string StorageKey, int Width, int Height);
public sealed record ImageFileDto(string Path, string ContentType);
