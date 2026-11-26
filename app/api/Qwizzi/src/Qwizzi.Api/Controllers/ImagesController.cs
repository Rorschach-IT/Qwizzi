using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Application.Images;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImagesController(IImageAssetService images) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null) return BadRequest(new { message = "An image file is required." });
        await using var content = file.OpenReadStream();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await images.UploadAsync(userId, file.FileName, file.ContentType, file.Length, content, ct);
        return result is null ? BadRequest(new { message = "Upload a square 512×512 JPEG image no larger than 2 MB." }) : Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{imageId:guid}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(Guid imageId, CancellationToken ct)
    {
        var image = await images.GetAsync(imageId, ct);
        return image is null ? NotFound() : PhysicalFile(image.Path, image.ContentType);
    }
}
