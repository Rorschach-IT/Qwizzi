using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Api.Authentication;
using Qwizzi.Application.Authentication;
using Qwizzi.Application.Authentication.DTOs;
using Qwizzi.Application.Security;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.AdministratorOnly)]
public sealed class AdminUsersController(IAccountService accounts, IAuthService auth, ISecurityAuditService audit) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateUserRequestDto request, CancellationToken ct)
    {
        var result = await auth.CreateByAdminAsync(request, ct);
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "UserCreated", "Administrator created a user account", result.IsSuccess, ct);
        if (!result.IsSuccess) return result.Error == AuthError.RegistrationConflict ? Conflict(new { message = "An account with the supplied details already exists." }) : BadRequest(new { message = "Check the submitted user details." });
        return StatusCode(StatusCodes.Status201Created, result.User);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string sortBy = "username", [FromQuery] string sortOrder = "asc", CancellationToken ct = default)
    {
        if (!new[] { "username", "status", "role", "createdAt"}.Contains(sortBy, StringComparer.OrdinalIgnoreCase)) return BadRequest(new { message = "sortBy must be username, status, role or createdAt." });
        if (!new[] { "asc", "desc" }.Contains(sortOrder, StringComparer.OrdinalIgnoreCase)) return BadRequest(new { message = "sortOrder must be asc or desc." });
        return Ok(await accounts.GetUsersAsync(sortBy, sortOrder, ct));
    }

    [HttpPatch("{userId:guid}/username")]
    public async Task<IActionResult> ChangeUsername(Guid userId, ChangeUsernameRequestDto request, CancellationToken ct) => await AuditedResult(await accounts.ChangeUserUsernameAsync(userId, request.Username, ct), "UsernameChanged", ct);

    [HttpPatch("{userId:guid}/email")]
    public async Task<IActionResult> ChangeEmail(Guid userId, ChangeEmailRequestDto request, CancellationToken ct) => await AuditedResult(await accounts.ChangeUserEmailAsync(userId, request.Email, ct), "EmailChanged", ct);

    [HttpPatch("{userId:guid}/password")]
    public async Task<IActionResult> SetPassword(Guid userId, AdminSetPasswordRequestDto request, CancellationToken ct)
    {
        var succeeded = await accounts.SetPasswordAsync(userId, request.Password, ct);
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "PasswordReset", "Administrator reset a user's password", succeeded, ct);
        return succeeded ? NoContent() : NotFound();
    }

    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid userId, [FromBody] bool status, CancellationToken ct)
    {
        if (!await accounts.SetStatusAsync(ActorId, userId, status, ct))
        {
            await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, status ? "UserActivationFailed" : "UserBlockFailed", "Administrator account-status change was rejected", false, ct);
            return Conflict(new { message = "User does not exist, cannot modify your own status, or this is the last active administrator." });
        }
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, status ? "UserActivated" : "UserBlocked", $"Administrator changed account status to {status}", true, ct);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken ct)
    {
        if (!await accounts.DeleteUserAsync(ActorId, userId, ct))
        {
            await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "UserDeletionFailed", "Administrator account deletion was rejected", false, ct);
            return Conflict(new { message = "User does not exist, cannot delete your own account, or this is the last active administrator." });
        }
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "UserDeleted", "Administrator deleted a user account", true, ct);
        return NoContent();
    }

    private IActionResult Result(object? result) => result is null ? Conflict(new { message = "Username or email is already in use, or the account does not exist." }) : Ok(result);
    private async Task<IActionResult> AuditedResult(object? result, string action, CancellationToken ct)
    {
        var succeeded = result is not null;
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, action, "Administrator changed user account details", succeeded, ct);
        return Result(result);
    }

    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
}
