using Aspire.Hosting;
using Aspire.Hosting.CommonGround.Maykin;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var unsafeTestPassword = builder.AddParameter("keycloakAdminPassword", "unsafe-test-password", secret: true);

var appAppClientId = builder.AddParameter("appClientId", "gpp-app");
var publicatiebankClientId = builder.AddParameter("publicatiebankClientId", "gpp-publicatiebank");

var adminUserName = builder.AddParameter("adminUserName", "admin");
var adminRoleName = builder.AddParameter("adminRoleName", "admin");

var keycloak = builder.AddKeycloak("keycloak", adminPassword: unsafeTestPassword)
    .WithRealmImport("./Realms")
    .WithEnvironment("APP_ADMIN_ROLE_NAME", adminRoleName)
    .WithEnvironment("APP_ADMIN_USER_NAME", adminUserName)
    .WithEnvironment("APP_ADMIN_USER_PASSWORD", unsafeTestPassword)
    .WithEnvironment("APP_CLIENT_ID", appAppClientId)
    .WithEnvironment("APP_CLIENT_SECRET", unsafeTestPassword)
    .WithEnvironment("APP_FRONTEND_URL", "http://localhost:5173")
    ;

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis");

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
    .WithArgs("-c", """
        mkdir /app/fixtures &&
        for f in /app/templates/*.json; do
            envsubst < "$f" > "/app/fixtures/$(basename "$f")"
        done &&
        /start.sh
        """)
    .WithEntrypoint("sh")
    .WithBindMount(source: "./templates/publicatiebank", target: "/app/templates", isReadOnly: true);

var publicatiebankProxy = publicatiebank.AddNginxProxy("publicatiebank-nginx");


var app = builder.AddProject<ODPC>("app")
    .WithReference(appDb)
    .WaitFor(appDb)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("OIDC_AUTHORITY", $"{keycloak.GetEndpoint("https")}/realms/app/")
    .WithEnvironment("OIDC_CLIENT_ID", appAppClientId)
    .WithEnvironment("OIDC_CLIENT_SECRET", unsafeTestPassword)
    .WithEnvironment("OIDC_ADMIN_ROLE", adminRoleName)
    //.WithEnvironment("OIDC_REQUIRE_HTTPS", "false")
    .WithEnvironment("POSTGRES_HOST", postgres.Resource.Host)
    .WithEnvironment("POSTGRES_PORT", postgres.Resource.Port)
    .WithEnvironment("POSTGRES_USER", postgres.Resource.UserNameReference)
    .WithEnvironment("POSTGRES_PASSWORD", postgres.Resource.PasswordParameter)
    .WithEnvironment("POSTGRES_DB", appDb.Resource.DatabaseName)
    .WithEnvironment("ODRC_BASE_URL", publicatiebankProxy.GetEndpoint("http"))
    .WithEnvironment("ODRC_API_KEY", unsafeTestPassword)
    ;

keycloak.WithEnvironment("APP_BACKEND_URL", $"http://localhost:{app.GetEndpoint("http").Property(EndpointProperty.Port)}");

builder.Build().Run();
