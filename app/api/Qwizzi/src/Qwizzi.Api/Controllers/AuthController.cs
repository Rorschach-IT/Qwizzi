using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Qwizzi.Api.Authentication;
using Qwizzi.Api.Security;
using Qwizzi.Application.Authentication;
using Qwizzi.Application.Authentication.DTOs;
using Qwizzi.Application.Security;

namespace Qwizzi.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None)]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ISecurityAuditService _audit;
    private readonly ITurnstileVerifier _turnstile;
    private readonly LoginChallengeTracker _loginChallenges;

    public AuthController(
        IAuthService authService,
        ISecurityAuditService audit,
        ITurnstileVerifier turnstile,
        LoginChallengeTracker loginChallenges)
    {
        _authService = authService;
        _audit = audit;
        _turnstile = turnstile;
        _loginChallenges = loginChallenges;
    }

    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult GetCsrfToken(
        [FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            token = tokens.RequestToken
        });
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Website))
        {
            var turnstileFailure = await VerifyTurnstileAsync(
                request.TurnstileToken,
                "register",
                "RegistrationTurnstileRejected",
                cancellationToken);
            if (turnstileFailure is not null)
            {
                return turnstileFailure;
            }
        }

        var result = await _authService.RegisterAsync(
            request,
            cancellationToken);
        await _audit.RecordAsync(result.User?.UserId, result.User?.Username, IpAddress, "Registration", "User registration", result.IsSuccess, cancellationToken);
        if (result.Error == AuthError.HoneypotTriggered)
        {
            await _audit.RecordAsync(null, null, IpAddress, "HoneypotTriggered", "Registration honeypot field was populated", false, cancellationToken);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.User);
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Website))
        {
            var challengeRequired = _loginChallenges.IsChallengeRequired(request.Email)
                                    || await _authService.IsLoginChallengeRequiredAsync(
                                        request.Email,
                                        cancellationToken);
            if (challengeRequired && string.IsNullOrWhiteSpace(request.TurnstileToken))
            {
                return TurnstileProblem(
                    StatusCodes.Status400BadRequest,
                    "turnstile_required",
                    "Complete the security check before trying again.");
            }

            if (!string.IsNullOrWhiteSpace(request.TurnstileToken))
            {
                var turnstileFailure = await VerifyTurnstileAsync(
                    request.TurnstileToken,
                    "login",
                    "LoginTurnstileRejected",
                    cancellationToken);
                if (turnstileFailure is not null)
                {
                    return turnstileFailure;
                }
            }
        }

        var result = await _authService.LoginAsync(
            request,
            cancellationToken);
        await _audit.RecordAsync(result.TargetUserId ?? result.User?.UserId, result.TargetUsername ?? result.User?.Username, IpAddress, "Login", "Password authentication", result.IsSuccess, cancellationToken);
        if (result.Error == AuthError.HoneypotTriggered)
        {
            await _audit.RecordAsync(null, null, IpAddress, "HoneypotTriggered", "Login honeypot field was populated", false, cancellationToken);
        }
        if (result.Error == AuthError.AccountLocked)
        {
            await _audit.RecordAsync(result.TargetUserId, result.TargetUsername, IpAddress, "AccountLocked", "Repeated failed sign-in attempts or an active lockout", false, cancellationToken);
        }

        if (result.Error == AuthError.InvalidCredentials)
        {
            _loginChallenges.RecordFailure(request.Email);
        }

        if (!result.IsSuccess)
        {
            return HandleFailure(result.Error);
        }

        _loginChallenges.Reset(request.Email);
        var principal = AuthPrincipalFactory.Create(
            result.User!);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false
            });

        return Ok(result.User);
    }

    [Authorize(Policy = AuthorizationPolicies.Authenticated)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _audit.RecordAsync(UserId, User.Identity?.Name, IpAddress, "Logout", "User signed out", true, HttpContext.RequestAborted);
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Authenticated)]
    [HttpGet("me")]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var userIdValue = User
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Unauthorized();
        }

        return Ok(user);
    }

    private IActionResult HandleFailure(AuthError error)
    {
        return error switch
        {
            AuthError.InvalidRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: "Check the submitted data."),

            AuthError.InvalidCredentials => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: "Invalid email or password."),

            AuthError.AccountLocked => Problem(
                statusCode: StatusCodes.Status423Locked,
                title: "Account temporarily locked",
                detail: "Too many unsuccessful sign-in attempts. Try again after the lockout expires."),

            AuthError.RegistrationConflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Registration failed",
                detail:
                    "An account with the supplied details already exists."),

            AuthError.HoneypotTriggered => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: "Check the submitted data."),

            _ => Problem(
                statusCode:
                    StatusCodes.Status500InternalServerError,
                title: "An unexpected error occurred.")
        };
    }

    private async Task<IActionResult?> VerifyTurnstileAsync(
        string token,
        string action,
        string auditAction,
        CancellationToken cancellationToken)
    {
        var result = await _turnstile.VerifyAsync(
            token,
            action,
            Request.Host.Host,
            cancellationToken);
        if (result == TurnstileVerificationStatus.Verified)
        {
            return null;
        }

        if (result == TurnstileVerificationStatus.Rejected)
        {
            await _audit.RecordAsync(
                null,
                null,
                IpAddress,
                auditAction,
                $"Cloudflare Turnstile rejected the {action} verification",
                false,
                cancellationToken);
            return TurnstileProblem(
                StatusCodes.Status400BadRequest,
                "turnstile_invalid",
                "The security check expired or could not be verified. Please try again.");
        }

        return TurnstileProblem(
            StatusCodes.Status503ServiceUnavailable,
            "turnstile_unavailable",
            "Security verification is temporarily unavailable. Please try again later.");
    }

    private ObjectResult TurnstileProblem(int statusCode, string code, string detail)
    {
        return new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = "Security verification failed",
            Detail = detail,
            Extensions = { ["code"] = code }
        })
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
    private Guid? UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}