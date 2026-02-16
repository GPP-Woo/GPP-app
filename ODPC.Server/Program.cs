using System.Net.Http.Headers;
using System.Text.Json.Nodes;
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

    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddHealthChecks();

    string GetRequiredConfig(string key)
    {
        var value = builder.Configuration[key];
        return string.IsNullOrWhiteSpace(value)
            ? throw new Exception($"Environment variable {key} is missing or empty")
            : value;
    };

    builder.Services.AddAuth(options =>
    {
        options.Authority = GetRequiredConfig("OIDC_AUTHORITY");
        options.ClientId = GetRequiredConfig("OIDC_CLIENT_ID");
        options.ClientSecret = GetRequiredConfig("OIDC_CLIENT_SECRET");
        options.AdminRole = GetRequiredConfig("OIDC_ADMIN_ROLE");
        options.NameClaimType = builder.Configuration["OIDC_NAME_CLAIM_TYPE"];
        options.RoleClaimType = builder.Configuration["OIDC_ROLE_CLAIM_TYPE"];
        options.IdClaimType = builder.Configuration["OIDC_ID_CLAIM_TYPE"];
    });

    var connStr = $"Username={builder.Configuration["POSTGRES_USER"]};Password={builder.Configuration["POSTGRES_PASSWORD"]};Host={builder.Configuration["POSTGRES_HOST"]};Database={builder.Configuration["POSTGRES_DB"]};Port={builder.Configuration["POSTGRES_PORT"]}";
    builder.Services.AddDbContext<OdpcDbContext>(opt => opt.UseNpgsql(connStr));
    builder.Services.AddScoped<IOdrcClientFactory, OdrcClientFactory>();
    builder.Services.AddScoped<IGebruikersgroepService, GebruikersgroepService>();
    // WooHoo calls an LLM which can take 30+ seconds — the default Aspire
    // resilience handler has a 10-second attempt timeout, causing premature cancellation.
#pragma warning disable EXTEXP0001
    builder.Services.AddHttpClient("WooHoo").RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

    var app = builder.Build();

    app.UseSerilogRequestLogging(x=> x.Logger = logger);
    app.UseDefaultFiles();
    app.UseOdpcStaticFiles();
    app.UseOdpcSecurityHeaders();

    app.UseAuthorization();

    app.MapControllers();

    app.MapOdpcAuthEndpoints();
    app.MapHealthChecks("/healthz").AllowAnonymous();
    UploadBestandsdeelEndpoint.Map(app);
    app.MapFallbackToIndexHtml();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<OdpcDbContext>();
        await db.Database.MigrateAsync();
    }

    // Seed a default gebruikersgroep in the background so it doesn't block app startup.
    // Guarded by SEED_ADMIN_GEBRUIKERSGROEP env var — never set in production.
    if (builder.Configuration["SEED_ADMIN_GEBRUIKERSGROEP"] == "true")
    {
        var config = builder.Configuration;
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(async () => await SeedBeheerders(app.Services, config, logger));
        });
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    logger.Write(LogEventLevel.Fatal, ex, "Application terminated unexpectedly");
}

static async Task SeedBeheerders(IServiceProvider services, IConfiguration config, Serilog.ILogger logger)
{
    try
    {
        logger.Information("Seeding admin gebruikersgroep for local development (ODRC: {Url})", config["ODRC_BASE_URL"]);

        var waardelijstUuids = await FetchAllWaardelijstUuids(config, logger);

        if (waardelijstUuids.Count == 0)
        {
            logger.Warning("No waardelijsten found from ODRC — admin group would have no permissions, skipping seed");
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OdpcDbContext>();

        if (db.Gebruikersgroepen.Any(g => g.Naam == "Beheerders")) return;

        var groep = new ODPC.Data.Entities.Gebruikersgroep
        {
            Uuid = Guid.NewGuid(),
            Naam = "Beheerders",
            Omschrijving = "Standaard gebruikersgroep voor beheerders"
        };
        db.Gebruikersgroepen.Add(groep);
        db.GebruikersgroepGebruikers.Add(new ODPC.Data.Entities.GebruikersgroepGebruiker
        {
            GebruikersgroepUuid = groep.Uuid,
            GebruikerId = "admin"
        });

        foreach (var uuid in waardelijstUuids)
        {
            db.GebruikersgroepWaardelijsten.Add(new ODPC.Data.Entities.GebruikersgroepWaardelijst
            {
                GebruikersgroepUuid = groep.Uuid,
                WaardelijstId = uuid
            });
        }

        await db.SaveChangesAsync();
        logger.Information("Seeded admin gebruikersgroep with {Count} waardelijsten", waardelijstUuids.Count);
    }
    catch (Exception ex)
    {
        logger.Warning(ex, "Failed to seed admin gebruikersgroep — app will start without it");
    }
}

static async Task<List<string>> FetchAllWaardelijstUuids(IConfiguration config, Serilog.ILogger logger)
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri(config["ODRC_BASE_URL"]!);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", config["ODRC_API_KEY"]);
    client.DefaultRequestHeaders.Add("Audit-User-ID", "seed");
    client.DefaultRequestHeaders.Add("Audit-User-Representation", "Local dev seed");
    client.DefaultRequestHeaders.Add("Audit-Remarks", "Fetching waardelijsten for admin group seed");

    // Retry because the ODRC (nginx proxy / publicatiebank) may still be starting up.
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            var uuids = new List<string>();
            string[] endpoints = ["organisaties", "informatiecategorieen", "onderwerpen"];

            foreach (var endpoint in endpoints)
            {
                var before = uuids.Count;
                var page = 1;
                bool hasMore = true;

                while (hasMore)
                {
                    using var response = await client.GetAsync($"/api/v2/{endpoint}?page={page}");
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadFromJsonAsync<PagedResponseModel<JsonNode>>();

                    if (result?.Results != null)
                    {
                        foreach (var item in result.Results)
                        {
                            var uuid = item?["uuid"]?.GetValue<string>();
                            if (uuid != null) uuids.Add(uuid);
                        }
                    }

                    hasMore = result?.Next != null;
                    page++;
                }

                logger.Information("Fetched {Count} {Endpoint} from ODRC", uuids.Count - before, endpoint);
            }

            return uuids.Distinct().ToList();
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "ODRC not reachable yet (attempt {Attempt}/10), retrying in 3s...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    return [];
}

public partial class Program { }
