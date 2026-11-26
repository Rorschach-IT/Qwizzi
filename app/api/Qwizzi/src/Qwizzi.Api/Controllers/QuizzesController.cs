using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Application.Quizzes;
using Qwizzi.Application.Quizzes.DTOs;
using Qwizzi.Application.Security;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
public sealed class QuizzesController(IQuizService quizzes, ISecurityAuditService audit) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] string? search, [FromQuery] int? categoryId, CancellationToken ct) =>
        Ok(await quizzes.BrowseAsync(search, categoryId, ct));

    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken ct) => Ok(await quizzes.GetCategoriesAsync(ct));

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await quizzes.GetOwnedAsync(UserId, ct));

    [HttpGet("{quizId:guid}/edit")]
    public async Task<IActionResult> EditModel(Guid quizId, CancellationToken ct)
    {
        var result = await quizzes.GetForEditingAsync(quizId, UserId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{quizId:guid}")]
    public async Task<IActionResult> Get(Guid quizId, CancellationToken ct)
    {
        var result = await quizzes.GetForPlayingAsync(quizId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] QuizWriteRequest request, CancellationToken ct)
    {
        var id = await quizzes.CreateAsync(UserId, request, ct);
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "QuizCreated", "User created a quiz pending moderation", id.HasValue, ct);
        return id.HasValue ? CreatedAtAction(nameof(Get), new { quizId = id.Value }, new { quizId = id.Value, status = 2 }) : BadRequest(new { message = "Quiz data is invalid, an image is unavailable, or the category does not exist." });
    }

    [HttpPut("{quizId:guid}")]
    public async Task<IActionResult> Update(Guid quizId, [FromBody] QuizWriteRequest request, CancellationToken ct)
    {
        var updated = await quizzes.UpdateAsync(quizId, UserId, request, ct);
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "QuizUpdated", "User updated a quiz; it requires moderation again", updated, ct);
        return updated ? NoContent() : Conflict(new { message = "Quiz not found, not owned by the current user, invalid, or has attempts that prevent changing its questions." });
    }

    [HttpDelete("{quizId:guid}")]
    public async Task<IActionResult> Delete(Guid quizId, CancellationToken ct)
    {
        var deleted = await quizzes.DeleteAsync(quizId, UserId, ct);
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "QuizDeleted", "User deleted a quiz", deleted, ct);
        return deleted ? NoContent() : NotFound();
    }

    [AllowAnonymous]
    [HttpPost("{quizId:guid}/attempts")]
    public async Task<IActionResult> Start(Guid quizId, CancellationToken ct)
    {
        var result = await quizzes.StartAsync(quizId, OptionalUserId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Guid? OptionalUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
}
