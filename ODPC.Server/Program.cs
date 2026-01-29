using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Data;
using ODPC.Features;
using ODPC.Features.Documenten.UploadBestandsdeel;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

using var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

logger.Information("Starting up");

try
{
    builder.Host.UseSerilog(logger);

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddHealthChecks();

    string GetRequiredConfig(string key)
    {
        var value = builder.Configuration[key];
        return string.IsNullOrWhiteSpace(value)
            ? throw new Exception($"Environment variable {key} is missing or empty")
            : value;
    }
    ;

    builder.Services.AddAuth(options =>
    {
        // For local dev, OIDC can be empty to disable authentication
        options.Authority = builder.Configuration["OIDC_AUTHORITY"] ?? string.Empty;
        options.ClientId = builder.Configuration["OIDC_CLIENT_ID"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["OIDC_CLIENT_SECRET"] ?? string.Empty;
        options.AdminRole = builder.Configuration["OIDC_ADMIN_ROLE"] ?? "admin";
        options.NameClaimType = builder.Configuration["OIDC_NAME_CLAIM_TYPE"];
        options.RoleClaimType = builder.Configuration["OIDC_ROLE_CLAIM_TYPE"];
        options.IdClaimType = builder.Configuration["OIDC_ID_CLAIM_TYPE"];
    });

    var connStr = $"Username={builder.Configuration["POSTGRES_USER"]};Password={builder.Configuration["POSTGRES_PASSWORD"]};Host={builder.Configuration["POSTGRES_HOST"]};Database={builder.Configuration["POSTGRES_DB"]};Port={builder.Configuration["POSTGRES_PORT"]}";
    builder.Services.AddDbContext<OdpcDbContext>(opt => opt.UseNpgsql(connStr));
    builder.Services.AddScoped<IOdrcClientFactory, OdrcClientFactory>();
    builder.Services.AddScoped<IGebruikerWaardelijstItemsService, GebruikerWaardelijstItemsService>();
    builder.Services.AddHttpClient("WooHoo");


    var app = builder.Build();

    app.UseSerilogRequestLogging(x => x.Logger = logger);
    app.UseDefaultFiles();
    app.UseOdpcStaticFiles();
    app.UseOdpcSecurityHeaders();

    app.UseDevAutoLogin(); // Must run BEFORE UseAuthentication to ensure cookie exists
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapOdpcAuthEndpoints();
    app.MapHealthChecks("/healthz").AllowAnonymous();
    UploadBestandsdeelEndpoint.Map(app);
    app.MapFallbackToIndexHtml();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        await scope.ServiceProvider.GetRequiredService<OdpcDbContext>().Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    logger.Write(LogEventLevel.Fatal, ex, "Application terminated unexpectedly");
}

public partial class Program { }
