using System.Security.Claims;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;

namespace ODPC.Authentication;

/// <summary>
/// Middleware that automatically signs in a local developer user when:
/// - Environment is Development
/// - OIDC is not configured
/// - User is not already authenticated
/// </summary>
public class DevAutoLoginMiddleware
{
    private const string CookieSchemeName = "cookieScheme";
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<DevAutoLoginMiddleware> _logger;

    public DevAutoLoginMiddleware(RequestDelegate next, IWebHostEnvironment env, IConfiguration config, ILogger<DevAutoLoginMiddleware> logger)
    {
        _next = next;
        _env = env;
        _config = config;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only auto-login in Development when OIDC is not configured
        if (_env.IsDevelopment() && string.IsNullOrWhiteSpace(_config["OIDC_AUTHORITY"]))
        {
            var adminRole = _config["OIDC_ADMIN_ROLE"] ?? "odpc-admin";

            // Use configured role claim type or default to "roles" (plural) - must match AuthenticationExtensions.cs
            var roleClaimType = _config["OIDC_ROLE_CLAIM_TYPE"];
            if (string.IsNullOrWhiteSpace(roleClaimType))
            {
                roleClaimType = JwtClaimTypes.Roles; // Plural to match default in AuthenticationExtensions.cs
            }

            // Always create/refresh the session on every request in dev mode
            // This ensures old/corrupted cookies don't cause issues
            var claims = new List<Claim>
            {
                new(JwtClaimTypes.PreferredUserName, "local-dev"),
                new(JwtClaimTypes.Name, "Local Developer"),
                new(JwtClaimTypes.Email, "local-dev@localhost"),
                new(roleClaimType, adminRole)
            };

            var identity = new ClaimsIdentity(claims, CookieSchemeName);
            var principal = new ClaimsPrincipal(identity);

            // Set context.User so this request sees the authenticated user
            context.User = principal;

            // Also create the cookie for future requests
            await context.SignInAsync(
                CookieSchemeName,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
        }

        await _next(context);
    }
}
