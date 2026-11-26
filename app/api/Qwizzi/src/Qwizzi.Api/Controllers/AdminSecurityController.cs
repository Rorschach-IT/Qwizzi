using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Api.Authentication;
using Qwizzi.Application.Authentication;
using Qwizzi.Application.Security;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/admin/security")]
[Authorize(Policy = AuthorizationPolicies.AdministratorOnly)]
public sealed class AdminSecurityController(IUserRepository users, ISecurityAuditService audit) : ControllerBase
{
    [HttpPost("password-policy/enforce")]
    public async Task<IActionResult> EnforcePasswordPolicy(CancellationToken ct)
    {
        var affected = await users.RequirePasswordChangeForAllAsync(ct);
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "PasswordPolicyEnforced", $"Forced password change for {affected} active users", true, ct);
        return Ok(new { affectedUsers = affected });
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] int limit = 100, CancellationToken ct = default)
    {
        if (limit is < 1 or > 500) return BadRequest(new { message = "limit must be between 1 and 500." });
        var events = await audit.GetRecentAsync(limit, ct);
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "SecurityLogExported", $"Administrator retrieved {events.Count} security events", true, ct);
        return Ok(events);
    }

    [HttpPost("events/verify")]
    public async Task<IActionResult> VerifyEvents(CancellationToken ct)
    {
        var result = await audit.VerifyAsync(ct);
        await audit.RecordAsync(ActorId, User.Identity?.Name, IpAddress, "SecurityLogVerified", result.IsIntact ? "Security log integrity verified" : $"Security log integrity failure at sequence {result.FirstInvalidSequence}", result.IsIntact, ct);
        return Ok(result);
    }

    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
}
