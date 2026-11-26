using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Api.Authentication;
using Qwizzi.Application.Quizzes;
using Qwizzi.Application.Quizzes.DTOs;
using Qwizzi.Application.Security;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/admin/quizzes")]
[Authorize(Policy = AuthorizationPolicies.AdministratorOnly)]
public sealed class AdminQuizzesController(IQuizService quizzes, ISecurityAuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await quizzes.GetAllForModerationAsync(ct));

    [HttpPatch("{quizId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid quizId, [FromBody] QuizModerationRequest request, CancellationToken ct)
    {
        var moderatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var succeeded = await quizzes.ModerateAsync(quizId, moderatorId, request, ct);
        await audit.RecordAsync(moderatorId, User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString(), "QuizModerated", $"Administrator changed quiz status to {request.Status}", succeeded, ct);
        return succeeded ? NoContent() : BadRequest(new { message = "Quiz not found, invalid status, or a rejection reason is required." });
    }
}
