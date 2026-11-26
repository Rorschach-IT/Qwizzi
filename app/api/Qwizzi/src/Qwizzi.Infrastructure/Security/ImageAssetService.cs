using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Qwizzi.Application.Abstractions.Persistence;
using Qwizzi.Application.Images;
using Qwizzi.Domain.Entities;
using Qwizzi.Infrastructure.Persistence;

namespace Qwizzi.Infrastructure.Security;

public sealed class ImageAssetService(AppDbContext dbContext, IUnitOfWork unitOfWork, IHostEnvironment environment, IConfiguration configuration) : IImageAssetService
{
    private const int TargetDimension = 512;
    private const int MaxFileSize = 2 * 1024 * 1024;
    private readonly string _rootDirectory = GetRootDirectory(environment, configuration);

    public async Task<ImageAssetDto?> UploadAsync(Guid userId, string fileName, string contentType, long length, Stream content, CancellationToken cancellationToken = default)
    {
        if (length is <= 0 or > MaxFileSize || !string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await using var memory = new MemoryStream((int)length);
        await content.CopyToAsync(memory, cancellationToken);
        if (memory.Length is <= 0 or > MaxFileSize) return null;

        var bytes = memory.ToArray();
        if (!TryReadJpegDimensions(bytes, out var width, out var height)
            || width != TargetDimension
            || height != TargetDimension)
        {
            return null;
        }

        var id = Guid.NewGuid();
        var storageKey = $"{id:N}.jpg";
        var path = Path.Combine(_rootDirectory, storageKey);
        Directory.CreateDirectory(_rootDirectory);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);

        var image = new ImageAsset
        {
            ImageAssetId = id,
            StorageKey = storageKey,
            OriginalFileName = Path.GetFileName(fileName)[..Math.Min(Path.GetFileName(fileName).Length, 255)],
            ContentType = "image/jpeg",
            SizeBytes = bytes.LongLength,
            Width = width,
            Height = height,
            UploadedById = userId
        };
        dbContext.ImageAssets.Add(image);
        var committed = false;
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            committed = true;
        }
        finally
        {
            if (!committed) File.Delete(path);
        }

        return new ImageAssetDto(id, storageKey, width, height);
    }

    public async Task<ImageFileDto?> GetAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await dbContext.ImageAssets.AsNoTracking()
            .SingleOrDefaultAsync(asset => asset.ImageAssetId == imageId, cancellationToken);
        if (image is null) return null;

        var path = Path.GetFullPath(Path.Combine(_rootDirectory, image.StorageKey));
        if (!path.StartsWith(_rootDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || !File.Exists(path))
        {
            return null;
        }

        return new ImageFileDto(path, image.ContentType);
    }

    private static bool TryReadJpegDimensions(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (bytes.Length < 4 || bytes[0] != 0xff || bytes[1] != 0xd8 || bytes[^2] != 0xff || bytes[^1] != 0xd9)
        {
            return false;
        }

        var index = 2;
        while (index < bytes.Length - 1)
        {
            if (bytes[index++] != 0xff) return false;
            while (index < bytes.Length && bytes[index] == 0xff) index++;
            if (index >= bytes.Length) return false;
            var marker = bytes[index++];
            if (marker is 0xd8 or 0xd9 or 0x01 or >= 0xd0 and <= 0xd7) continue;
            if (index + 2 > bytes.Length) return false;

            var segmentLength = (bytes[index] << 8) | bytes[index + 1];
            if (segmentLength < 2 || index + segmentLength > bytes.Length) return false;
            if (marker is 0xc0 or 0xc1 or 0xc2 or 0xc3 or 0xc5 or 0xc6 or 0xc7 or 0xc9 or 0xca or 0xcb or 0xcd or 0xce or 0xcf)
            {
                if (segmentLength < 7) return false;
                height = (bytes[index + 3] << 8) | bytes[index + 4];
                width = (bytes[index + 5] << 8) | bytes[index + 6];
                return width > 0 && height > 0;
            }

            index += segmentLength;
        }

        return false;
    }

    private static string GetRootDirectory(IHostEnvironment environment, IConfiguration configuration)
    {
        var configuredPath = configuration["ImageStorage:RootPath"];
        return Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "images")
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath));
    }
}
