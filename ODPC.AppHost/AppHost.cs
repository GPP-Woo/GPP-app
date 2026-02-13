using Aspire.Hosting;
using Aspire.Hosting.CommonGround.Maykin;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var unsafeTestPassword = builder.AddParameter("unsafeTestPassword", "admin", secret: true);

var appAppClientId = builder.AddParameter("appClientId", "gpp-app");
var publicatiebankClientId = builder.AddParameter("publicatiebankClientId", "gpp-publicatiebank");

var adminUserName = builder.AddParameter("adminUserName", "admin");
var adminRoleName = builder.AddParameter("adminRoleName", "admin");

var keycloak = builder.AddKeycloak("keycloak", adminPassword: unsafeTestPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRealmImport("./Realms")
    .WithEnvironment("APP_ADMIN_ROLE_NAME", adminRoleName)
    .WithEnvironment("APP_ADMIN_USER_NAME", adminUserName)
    .WithEnvironment("APP_ADMIN_USER_PASSWORD", unsafeTestPassword)
    .WithEnvironment("APP_CLIENT_ID", appAppClientId)
    .WithEnvironment("APP_CLIENT_SECRET", unsafeTestPassword)
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    ;

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImage("postgis/postgis")
    .WithPgAdmin(a => a.WithLifetime(ContainerLifetime.Persistent))
    .WithDataVolume();

var redis = builder.AddRedis("redis");

var appDb = postgres.AddDatabase("app-db");

var openzaakDb = postgres.AddDatabase("openzaak-db");

var openzaak = builder.AddOpenZaak("openzaak")
    .WithReference(openzaakDb)
    .WaitFor(openzaakDb)
    .WithReference(redis)
    .WaitFor(redis);



var publicatiebankDb = postgres.AddDatabase("publicatiebank-db");

var publicatiebank = builder.AddPublicatiebank("publicatiebank")
    .WithReference(publicatiebankDb)
    .WaitFor(publicatiebankDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithEnvironment("APP_API_KEY", unsafeTestPassword)
    .WithEnvironment("OPENZAAK_CLIENT_ID", publicatiebankClientId)
    .WithEnvironment("OPENZAAK_CLIENT_SECRET", unsafeTestPassword)
    .WithEnvironment("OPENZAAK_BASE_URL", openzaak.GetEndpoint("http"))
    .WithTemplateFixtures("./templates/publicatiebank");

var publicatiebankCelery = publicatiebank.AddCeleryWorker("publicatiebank-celery");

openzaak
    .WithEnvironment("PUBLICATIEBANK_BASE_URL", publicatiebank.GetEndpoint("http"))
    .WithEnvironment("PUBLICATIEBANK_CLIENT_ID", publicatiebankClientId)
    .WithEnvironment("PUBLICATIEBANK_CLIENT_SECRET", unsafeTestPassword)
    .WithTemplateFixtures("./templates/openzaak");

var publicatiebankProxy = publicatiebank.AddNginxProxy("publicatiebank-nginx");

var llmApiKey = builder.AddParameter("llmApiKey", secret: true);

var wooHoo = builder.AddDockerfile("woo-hoo", "../../woo-hoo")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(targetPort: 8000)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("LLM_API_KEY", llmApiKey)
    .WithEnvironment("LLM_PROVIDER", "openrouter")
    .WithEnvironment("DEFAULT_MODEL", "mistralai/mistral-large-2512")
    .WithEnvironment("FALLBACK_MODEL", "mistralai/mistral-small-3.2-24b-instruct-2506")
    .WithEnvironment("MAX_TEXT_LENGTH", "15000")
    .WithEnvironment("LLM_TEMPERATURE", "0.1")
    .WithEnvironment("LLM_TIMEOUT_SECONDS", "60")
    .WithEnvironment("LLM_MAX_RETRIES", "3")
    .WithEnvironment("GPP_PUBLICATIEBANK_URL", publicatiebank.GetEndpoint("http"))
    .WithEnvironment("GPP_API_TOKEN", unsafeTestPassword)
    .WithEnvironment("LOG_LEVEL", "INFO")
    .WithEnvironment("LOG_FORMAT", "console");

var app = builder.AddProject<ODPC>("app")
    .WithReference(appDb)
    .WaitFor(appDb)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("OIDC_AUTHORITY", $"{keycloak.GetEndpoint("https")}/realms/app/")
    .WithEnvironment("OIDC_CLIENT_ID", appAppClientId)
    .WithEnvironment("OIDC_CLIENT_SECRET", unsafeTestPassword)
    .WithEnvironment("OIDC_ADMIN_ROLE", adminRoleName)
    .WithEnvironment("POSTGRES_HOST", postgres.Resource.Host)
    .WithEnvironment("POSTGRES_PORT", postgres.Resource.Port)
    .WithEnvironment("POSTGRES_USER", postgres.Resource.UserNameReference)
    .WithEnvironment("POSTGRES_PASSWORD", postgres.Resource.PasswordParameter)
    .WithEnvironment("POSTGRES_DB", appDb.Resource.DatabaseName)
    .WithEnvironment("ODRC_BASE_URL", publicatiebankProxy.GetEndpoint("http"))
    .WithEnvironment("ODRC_API_KEY", unsafeTestPassword)
    .WithEnvironment("WOO_HOO_BASE_URL", wooHoo.GetEndpoint("http"))
    .WithEnvironment("WOO_HOO_HEALTH_TIMEOUT_SECONDS", "30")
    .WithEnvironment("WOO_HOO_GENERATE_TIMEOUT_SECONDS", "120")
    .WithEnvironment("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true")
    ;

var vite = builder.AddViteApp("vite", "../odpc.client")
    .WithReference(app);

builder.Build().Run();
