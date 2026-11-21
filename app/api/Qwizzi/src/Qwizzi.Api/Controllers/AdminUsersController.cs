using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Api.Authentication;
using Qwizzi.Application.Authentication;
using Qwizzi.Application.Authentication.DTOs;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.AdministratorOnly)]
public sealed class AdminUsersController(IAccountService accounts) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string sortBy = "username", [FromQuery] string sortOrder = "asc", CancellationToken ct = default)
    {
        if (!new[] { "username", "status", "role", "createdAt"}.Contains(sortBy, StringComparer.OrdinalIgnoreCase)) return BadRequest(new { message = "sortBy must be username, status, role or createdAt." });
        if (!new[] { "asc", "desc" }.Contains(sortOrder, StringComparer.OrdinalIgnoreCase)) return BadRequest(new { message = "sortOrder must be asc or desc." });
        return Ok(await accounts.GetUsersAsync(sortBy, sortOrder, ct));
    }

    [HttpPatch("{userId:guid}/username")]
    public async Task<IActionResult> ChangeUsername(Guid userId, ChangeUsernameRequestDto request, CancellationToken ct) => Result(await accounts.ChangeUserUsernameAsync(userId, request.Username, ct));

    [HttpPatch("{userId:guid}/email")]
    public async Task<IActionResult> ChangeEmail(Guid userId, ChangeEmailRequestDto request, CancellationToken ct) => Result(await accounts.ChangeUserEmailAsync(userId, request.Email, ct));

    [HttpPatch("{userId:guid}/password")]
    public async Task<IActionResult> SetPassword(Guid userId, AdminSetPasswordRequestDto request, CancellationToken ct) => await accounts.SetPasswordAsync(userId, request.Password, ct) ? NoContent() : NotFound();

    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid userId, [FromBody] bool status, CancellationToken ct)
    {
        if (!await accounts.SetStatusAsync(ActorId, userId, status, ct)) return Conflict(new { message = "User does not exist, cannot modify your own status, or this is the last active administrator." });
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken ct)
    {
        if (!await accounts.DeleteUserAsync(ActorId, userId, ct)) return Conflict(new { message = "User does not exist, cannot delete your own account, or this is the last active administrator." });
        return NoContent();
    }

    private IActionResult Result(object? result) => result is null ? Conflict(new { message = "Username or email is already in use, or the account does not exist." }) : Ok(result);
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
