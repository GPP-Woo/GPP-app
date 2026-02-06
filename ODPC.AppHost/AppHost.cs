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
    .WithImage("postgis/postgis")
    .WithPgAdmin()
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
    .WithArgs("-c", """
        mkdir /app/fixtures &&
        for f in /app/templates/*.json; do
            envsubst < "$f" > "/app/fixtures/$(basename "$f")"
        done &&
        /start.sh
        """)
    .WithEntrypoint("sh")
    .WithBindMount(source: "./templates/publicatiebank", target: "/app/templates", isReadOnly: true);

openzaak
    .WithEnvironment("PUBLICATIEBANK_BASE_URL", publicatiebank.GetEndpoint("http"))
    .WithEnvironment("PUBLICATIEBANK_CLIENT_ID", publicatiebankClientId)
    .WithEnvironment("PUBLICATIEBANK_CLIENT_SECRET", unsafeTestPassword)
    .WithArgs("-c", """
        mkdir /app/fixtures &&
        for f in /app/templates/*.json; do
            envsubst < "$f" > "/app/fixtures/$(basename "$f")"
        done &&
        /start.sh
        """)
    .WithEntrypoint("sh")
    .WithBindMount(source: "./templates/openzaak", target: "/app/templates", isReadOnly: true);

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
    .WithEnvironment("POSTGRES_HOST", postgres.Resource.Host)
    .WithEnvironment("POSTGRES_PORT", postgres.Resource.Port)
    .WithEnvironment("POSTGRES_USER", postgres.Resource.UserNameReference)
    .WithEnvironment("POSTGRES_PASSWORD", postgres.Resource.PasswordParameter)
    .WithEnvironment("POSTGRES_DB", appDb.Resource.DatabaseName)
    .WithEnvironment("ODRC_BASE_URL", publicatiebankProxy.GetEndpoint("http"))
    .WithEnvironment("ODRC_API_KEY", unsafeTestPassword)
    .WithEnvironment("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true")
    ;

var vite = builder.AddViteApp("vite", "../odpc.client")
    .WithReference(app);

builder.Build().Run();
