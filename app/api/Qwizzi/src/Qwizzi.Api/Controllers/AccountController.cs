using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Api.Authentication;
using Qwizzi.Api.Security;
using Qwizzi.Application.Authentication;
using Qwizzi.Application.Authentication.DTOs;
using Qwizzi.Application.Security;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class AccountController(
    IAccountService accounts,
    ISecurityAuditService audit,
    ITurnstileVerifier turnstile) : ControllerBase
{
    [HttpPatch("username")]
    public async Task<IActionResult> ChangeUsername(ChangeUsernameRequestDto request, CancellationToken ct)
    {
        var result = await accounts.ChangeUsernameAsync(UserId, request.Username, ct);
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "UsernameChanged", "User changed their username", result is not null, ct);
        return Result(result);
    }

    [HttpPatch("email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequestDto request, CancellationToken ct)
    {
        var result = await accounts.ChangeEmailAsync(UserId, request.Email, ct);
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "EmailChanged", "User changed their email address", result is not null, ct);
        return Result(result);
    }

    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request, CancellationToken ct)
    {
        var verification = await turnstile.VerifyAsync(
            request.TurnstileToken,
            "password-change",
            Request.Host.Host,
            ct);
        if (verification != TurnstileVerificationStatus.Verified)
        {
            if (verification == TurnstileVerificationStatus.Rejected)
            {
                await audit.RecordAsync(
                    UserId,
                    User.Identity?.Name,
                    IpAddress,
                    "PasswordChangeTurnstileRejected",
                    "Cloudflare Turnstile rejected password change verification",
                    false,
                    ct);
                return BadRequest(new { message = "The security check expired or could not be verified. Please try again." });
            }

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Security verification is temporarily unavailable. Please try again later." });
        }

        var succeeded = await accounts.ChangeOwnPasswordAsync(UserId, request.CurrentPassword, request.NewPassword, ct);
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "PasswordChanged", "User changed their password", succeeded, ct);
        if (!succeeded) return BadRequest(new { message = "The current password is incorrect." });
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        if (!await accounts.DeleteOwnAsync(UserId, ct)) return NotFound();
        await audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "AccountDeleted", "User deleted their account", true, ct);
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    private IActionResult Result(object? result) => result is null ? Conflict(new { message = "Username or email is already in use, or the account does not exist." }) : Ok(result);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
}
