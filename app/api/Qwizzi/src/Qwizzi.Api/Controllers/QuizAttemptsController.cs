using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Application.Quizzes;
using Qwizzi.Application.Quizzes.DTOs;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/quiz-attempts")]
public sealed class QuizAttemptsController(IQuizService quizzes) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("{attemptId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid attemptId, [FromBody] SubmitQuizRequest request, CancellationToken ct)
    {
        var result = await quizzes.SubmitAsync(attemptId, OptionalUserId, request, ct);
        return result is null ? Conflict(new { message = "The attempt is invalid, already completed, or the answers do not match its questions." }) : Ok(result);
    }

    private Guid? OptionalUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
