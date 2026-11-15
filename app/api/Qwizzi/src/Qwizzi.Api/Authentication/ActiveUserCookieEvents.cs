using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Qwizzi.Application.Authentication;

namespace Qwizzi.Api.Authentication;

public sealed class ActiveUserCookieEvents(IAuthService authService)
    : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        var userIdValue = context.Principal?
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            await RejectAsync(context);
            return;
        }

        var user = await authService.GetCurrentUserAsync(
            userId,
            context.HttpContext.RequestAborted);

        var cookieRole = context.Principal?
            .FindFirst(ClaimTypes.Role)?
            .Value;

        if (user is null || cookieRole != user.Role)
        {
            await RejectAsync(context);
        }
    }

    public override Task RedirectToLogin(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    }

    private static async Task RejectAsync(
        CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();

        await context.HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}