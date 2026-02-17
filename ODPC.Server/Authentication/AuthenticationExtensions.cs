using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ODPC.Data;
using ODPC.Data.Entities;

namespace ODPC.Authentication
{

    public static class AuthenticationExtensions
    {
        private const string SignOutCallback = "/signout-callback-oidc";
        private const string CookieSchemeName = "cookieScheme";
        private const string ChallengeSchemeName = "challengeScheme";

        public static void AddAuth(this IServiceCollection services, Action<AuthOptions> setOptions)
        {
            var authOptions = new AuthOptions();
            setOptions(authOptions);

            var nameClaimType = string.IsNullOrWhiteSpace(authOptions.NameClaimType) ? JwtClaimTypes.Name : authOptions.NameClaimType;
            var roleClaimType = string.IsNullOrWhiteSpace(authOptions.RoleClaimType) ? JwtClaimTypes.Roles : authOptions.RoleClaimType;
            string[] idClaimTypes = string.IsNullOrWhiteSpace(authOptions.IdClaimType) ? [JwtClaimTypes.PreferredUserName, JwtClaimTypes.Email] : [authOptions.IdClaimType];

            services.AddHttpContextAccessor();

            services.AddScoped<OdpcUser>(s =>
            {
                var user = s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User;
                var isLoggedIn = user?.Identity?.IsAuthenticated ?? false;
                var name = user?.FindFirst(nameClaimType)?.Value;
                var id = user?.FindFirst(x => idClaimTypes.Contains(x.Type))?.Value;
                var roles = user?.FindAll(roleClaimType).Select(x=> x.Value).ToArray() ?? [];
                var isAdmin = roles.Contains(authOptions.AdminRole);
                return new OdpcUser { IsLoggedIn = isLoggedIn, FullName = name, Id = id, Roles = roles, IsAdmin = isAdmin };
            });

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieSchemeName;
                if (!string.IsNullOrWhiteSpace(authOptions.Authority))
                {
                    options.DefaultChallengeScheme = ChallengeSchemeName;
                }
            }).AddCookie(CookieSchemeName, options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                // TODO: make configurable?
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                //options.Events.OnSigningOut = (e) => e.HttpContext.RevokeRefreshTokenAsync();
                options.Events.OnSigningOut = (ctx) =>
                {
                    ctx.Response.Redirect("/");
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = (ctx) =>
                {
                    //https://www.rfc-editor.org/rfc/rfc7231#section-6.5.3
                    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogin = HandleLoggedOut;
            });

            if (!string.IsNullOrWhiteSpace(authOptions.Authority))
            {
                authBuilder.AddOpenIdConnect(ChallengeSchemeName, options =>
                {
                    options.NonceCookie.HttpOnly = true;
                    options.NonceCookie.IsEssential = true;
                    options.NonceCookie.SameSite = SameSiteMode.None;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.CorrelationCookie.HttpOnly = true;
                    options.CorrelationCookie.IsEssential = true;
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.Authority = authOptions.Authority;
                    options.ClientId = authOptions.ClientId;
                    options.ClientSecret = authOptions.ClientSecret;
                    options.SignedOutRedirectUri = SignOutCallback;
                    options.ResponseType = OidcConstants.ResponseTypes.Code;
                    options.UsePkce = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Clear();
                    options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                    options.Scope.Add(OidcConstants.StandardScopes.Profile);
                    //options.Scope.Add(OidcConstants.StandardScopes.OfflineAccess);
                    //options.SaveTokens = true;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RoleClaimType = roleClaimType,
                    };


                    options.Events.OnRemoteFailure = RedirectToRoot;
                    options.Events.OnSignedOutCallbackRedirect = RedirectToRoot;
                    options.Events.OnRedirectToIdentityProvider = (ctx) =>
                    {
                        if (ctx.Request.Headers.ContainsKey("is-api"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.Response.Headers.Location = ctx.ProtocolMessage.CreateAuthenticationRequestUrl();
                            ctx.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };

                    //naam gebruiker bijwerken, draait 1x per login
                    options.Events.OnTicketReceived = async (ctx) =>
                    {
                        var id = ctx.Principal?.FindFirst(x => idClaimTypes.Contains(x.Type))?.Value;
                        var name = ctx.Principal?.FindFirst(nameClaimType)?.Value;

                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            var dbContext = ctx.HttpContext.RequestServices.GetRequiredService<OdpcDbContext>();
                            var gebruiker = await dbContext.Gebruikers.SingleOrDefaultAsync(g => g.GebruikerId == id);

                            if (gebruiker == null)
                            {
                                dbContext.Gebruikers.Add(new Gebruiker
                                {
                                    GebruikerId = id,
                                    Naam = name,
                                    LastLogin = DateTimeOffset.UtcNow
                                });
                            }
                            else
                            {
                                gebruiker.Naam = name;
                                gebruiker.LastLogin = DateTimeOffset.UtcNow;
                            }

                            await dbContext.SaveChangesAsync();
                        }
                    };
                });
            }
            services.AddAuthorizationBuilder()
                .AddPolicy(AdminPolicy.Name, policy => policy.RequireRole(authOptions.AdminRole))
                .AddFallbackPolicy("LoggedIn", policy => policy.RequireAuthenticatedUser());
            services.AddDistributedMemoryCache();
            services.AddOpenIdConnectAccessTokenManagement();
        }

        public static IEndpointRouteBuilder MapOdpcAuthEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/logoff", LogoffAsync).AllowAnonymous();
            endpoints.MapGet("api/me", (OdpcUser user) => user).AllowAnonymous();
            endpoints.MapGet("api/challenge", ChallengeAsync).AllowAnonymous();

            return endpoints;
        }

        private static async Task LogoffAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieSchemeName);
        }

        private static Task RedirectToRoot<TOptions>(HandleRequestContext<TOptions> context) where TOptions : AuthenticationSchemeOptions
        {
            context.Response.Redirect("/");
            context.HandleResponse();

            return Task.CompletedTask;
        }

        private static Task HandleLoggedOut<TOptions>(RedirectContext<TOptions> ctx) where TOptions : AuthenticationSchemeOptions
        {
            if (ctx.Request.Headers.ContainsKey("is-api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.Headers.Location = ctx.RedirectUri;
            }
            return Task.CompletedTask;
        }

        private static Task ChallengeAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var returnPath = GetRelativeReturnUrl(request);

            if (httpContext.User.Identity?.IsAuthenticated ?? false)
            {
                httpContext.Response.Redirect(returnPath);
                return Task.CompletedTask;
            }

            return httpContext.ChallengeAsync(new AuthenticationProperties
            {
                RedirectUri = returnPath,
            });
        }

        /// <summary>
        /// We gebruiken een query parameter om te bepalen waar we naartoe moeten redirecten na inlog.
        /// Dat is gebruikersinput. Daarom willen we valideren dat die query parameter daadwerkelijk een relatieve url is.
        /// Zo niet, redirecten we naar de root van de applicatie.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetRelativeReturnUrl(HttpRequest request)
        {
            var returnUrl = request.Query["returnUrl"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(returnUrl) || new Uri(returnUrl, UriKind.RelativeOrAbsolute).IsAbsoluteUri) return "/";
            return $"/{returnUrl.AsSpan().TrimStart('/')}";
        }
    }
}
