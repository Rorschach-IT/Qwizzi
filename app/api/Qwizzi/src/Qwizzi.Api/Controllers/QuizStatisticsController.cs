using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Application.Quizzes;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/account/statistics/quizzes")]
public sealed class QuizStatisticsController(IQuizService quizzes) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await quizzes.GetStatisticsAsync(userId, ct));
    }
}
