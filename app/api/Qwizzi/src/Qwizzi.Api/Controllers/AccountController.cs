using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qwizzi.Api.Authentication;
using Qwizzi.Application.Authentication;
using Qwizzi.Application.Authentication.DTOs;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class AccountController(IAccountService accounts) : ControllerBase
{
    [HttpPatch("username")]
    public async Task<IActionResult> ChangeUsername(ChangeUsernameRequestDto request, CancellationToken ct) =>
        Result(await accounts.ChangeUsernameAsync(UserId, request.Username, ct));

    [HttpPatch("email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequestDto request, CancellationToken ct) =>
        Result(await accounts.ChangeEmailAsync(UserId, request.Email, ct));

    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request, CancellationToken ct)
    {
        if (!await accounts.ChangeOwnPasswordAsync(UserId, request.CurrentPassword, request.NewPassword, ct)) return BadRequest(new { message = "The current password is incorrect." });
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        if (!await accounts.DeleteOwnAsync(UserId, ct)) return NotFound();
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    private IActionResult Result(object? result) => result is null ? Conflict(new { message = "Username or email is already in use, or the account does not exist." }) : Ok(result);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
