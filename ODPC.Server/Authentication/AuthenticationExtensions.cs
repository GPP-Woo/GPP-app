using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

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
                var roles = user?.FindAll(roleClaimType).Select(x => x.Value).ToArray() ?? [];
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
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in dev, HTTPS in prod
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
                });
            }

            var authzBuilder = services.AddAuthorizationBuilder();

            // In dev mode without OIDC, admin policy just requires being authenticated
            // In production with OIDC, admin policy requires the actual admin role
            if (!string.IsNullOrWhiteSpace(authOptions.Authority))
            {
                authzBuilder.AddPolicy(AdminPolicy.Name, policy => policy.RequireRole(authOptions.AdminRole));
                authzBuilder.AddFallbackPolicy("LoggedIn", policy => policy.RequireAuthenticatedUser());
            }
            else
            {
                // Dev mode: admin policy just requires any authenticated user
                authzBuilder.AddPolicy(AdminPolicy.Name, policy => policy.RequireAuthenticatedUser());
            }

            services.AddDistributedMemoryCache();
            services.AddOpenIdConnectAccessTokenManagement();
        }

        public static IApplicationBuilder UseDevAutoLogin(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DevAutoLoginMiddleware>();
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
