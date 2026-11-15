using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Qwizzi.Api.Authentication;
using Qwizzi.Application.Abstractions.Persistence;
using Qwizzi.Application.Authentication;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;
using Qwizzi.Infrastructure.Persistence;
using Qwizzi.Infrastructure.Persistence.Repositories;
using Qwizzi.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);


// Registers services used to generate the OpenAPI document.
// The document endpoint and Swagger UI are exposed only in Development.
builder.Services.AddOpenApi();


// Loads the PostgreSQL connection string and stops startup
// immediately if the required configuration is missing.
var connectionString =
    builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "There is no connection string: ConnectionStrings:Database.");


// Registers one scoped EF Core context for database access.
// Repositories and the unit of work share this context within a request.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// Registers the event handler that checks whether the account
// represented by an existing authentication cookie is still valid.
builder.Services.AddScoped<ActiveUserCookieEvents>();


// Configures cookie-based authentication.
//
// HttpOnly prevents JavaScript from reading the authentication cookie.
// Secure requires HTTPS.
// SameSite=Lax is the baseline for the current same-site setup.
// The __Host- prefix requires Secure, Path=/, and no Domain attribute.
//
// Cookie validation is delegated to ActiveUserCookieEvents.
// Expiration is fixed because sliding expiration is disabled.
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-Qwizzi.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;

        options.EventsType = typeof(ActiveUserCookieEvents);
    });


// Defines authorization policies.
//
// Authenticated accepts any authenticated user, including administrators.
// AdministratorOnly additionally requires the Administrator role.
//
// The fallback policy protects endpoints by default.
// Intentionally public endpoints must use [AllowAnonymous]
// or the AllowAnonymous() endpoint convention.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.Authenticated,
        policy => policy.RequireAuthenticatedUser());

    options.AddPolicy(
        AuthorizationPolicies.AdministratorOnly,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(nameof(UserRole.Administrator)));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


// Registers the EF Core implementation of user persistence operations.
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Resolves IUnitOfWork to the existing scoped AppDbContext.
// This avoids creating a separate context for committing changes.
builder.Services.AddScoped<IUnitOfWork>(provider =>
    provider.GetRequiredService<AppDbContext>());


// Registers the framework password hasher and the application-facing
// adapter responsible for hashing and verifying user passwords.
builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<
    IPasswordService,
    PasswordService>();


// Registers application-level registration and login workflows.
// HTTP responses and authentication cookies remain the controller's
// responsibility.
builder.Services.AddScoped<IAuthService, AuthService>();


// Registers controllers and the MVC services required
// by the built-in antiforgery validation filter.
// The application can still return JSON without using Razor views.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(
        new AutoValidateAntiforgeryTokenAttribute());
});


// Configures the antiforgery cookie and the request-token header.
//
// The client obtains a request token from GET /api/auth/csrf,
// preserves the accompanying cookie, and sends the token
// in the X-CSRF-TOKEN header when modifying server-side state.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";

    options.Cookie.Name = "__Host-Qwizzi.Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
});


// Defines an IP-partitioned fixed-window rate limit.
//
// Endpoints marked with [EnableRateLimiting("auth")] share a limit
// of 10 requests per minute for each client IP address.
// Requests exceeding the limit are rejected immediately with HTTP 429.
//
// This limiter is local to the current application instance.
// Reverse-proxy deployments require trusted forwarded-header configuration
// before the remote IP address can reliably identify the client.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",

            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});


// Builds the application after all services have been registered.
var app = builder.Build();


// Redirects HTTP requests to HTTPS.
// Clients should call HTTPS directly, especially when sending credentials.
app.UseHttpsRedirection();


// Exposes API documentation only in the Development environment.
//
// The OpenAPI document is explicitly public so that the fallback
// authorization policy does not prevent loading it before login.
// Swagger UI is registered before authentication and authorization.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
        .AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Qwizzi API v1");

        options.RoutePrefix = "swagger";
    });
}


// Selects the endpoint and makes its metadata available
// to authorization and endpoint-specific rate limiting.
app.UseRouting();


// If a separate React origin is introduced later,
// configure a specific CORS policy and apply it here.


// Reads and validates the authentication cookie,
// then populates HttpContext.User.
app.UseAuthentication();


// Enforces endpoint authorization policies using the current identity.
app.UseAuthorization();


// Enforces rate-limiting policies selected by endpoint metadata.
// AddRateLimiter registers the services; this middleware executes them.
app.UseRateLimiter();


// Maps attribute-routed controller actions.
// MVC filters, including antiforgery validation, run during execution.
app.MapControllers();


// Starts processing incoming requests.
app.Run();