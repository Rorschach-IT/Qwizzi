namespace Qwizzi.Seeder;

public static class SeedSettings
{
    // Paths are relative to the backend root containing Qwizzi.slnx.
    public const string SourceImagesRelativePath =
        "seed/images/" + EnvironmentPicker.EnvironmentSelection;

    // Proposed destination for local development.
    // It must match the storage location used by the API.
    public const string StorageRelativePath =
        "storage/images";

    public const string StorageKeyPrefix =
        "seed/demo-v1";

    public const string RejectionReason = "temporary mistake in algorithm";

    public static readonly string[] RequiredImageFileNames =
    [
        "image-01.webp",
        "image-02.webp",
        "image-03.webp",
        "image-04.webp",
        "image-05.webp",
        "image-06.webp",
        "image-07.webp"
    ];
}
