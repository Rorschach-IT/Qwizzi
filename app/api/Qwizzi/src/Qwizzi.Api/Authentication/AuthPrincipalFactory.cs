using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Qwizzi.Application.Authentication.DTOs;

namespace Qwizzi.Api.Authentication;

public static class AuthPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticatedUserDto user)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}