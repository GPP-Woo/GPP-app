using System.Reflection;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.CommonGround.Maykin;

public abstract class MaykinResource(string name, string? entrypoint = null) : ContainerResource(name, entrypoint);

public static class MaykinExtensions
{
    private static int redisDbCounter = -1;
    private static readonly string s_executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("could not get executing directory");
    private static readonly string s_nginxPath = Path.Combine(s_executionPath, "nginx", "templates", "default.conf.template");

    public static IResourceBuilder<T> WithMaykinDefaults<T>(this IResourceBuilder<T> maykin, int? port = null) where T : MaykinResource
    {
        maykin = maykin.WithHttpEndpoint(port: port, targetPort: 8000);

        var endpoint = maykin.GetEndpoint("http");

        return maykin
            .WithContainerRuntimeArgs("--platform", "linux/amd64")
            .WithHttpHealthCheck("/admin")
            .WithOtlpExporter()
            .WithCertificateTrustConfiguration(ctx =>
            {
                ctx.EnvironmentVariables["EXTRA_VERIFY_CERTS"] = ctx.CertificateBundlePath;
                ctx.EnvironmentVariables["OTEL_EXPORTER_OTLP_CERTIFICATE"] = ctx.CertificateBundlePath;
                return Task.CompletedTask;
            })
            .WithEnvironment("SECRET_KEY", "7(h1r2hk)8z9+05edulo_3qzymwbo&c24=)qz7+_@3&2sp=u%i")
            .WithEnvironment("IS_HTTPS", "no")
            .WithEnvironment("SITE_DOMAIN", $"localhost:{endpoint.Property(EndpointProperty.Port)}")
            .WithEnvironment("ALLOWED_HOSTS", $"localhost,127.0.0.1,{endpoint.Property(EndpointProperty.Host)}")
            .WithEnvironment("CORS_ALLOW_ALL_ORIGINS", "True")
            .WithEnvironment("SUBPATH", "/")
            .WithEnvironment("DJANGO_SUPERUSER_PASSWORD", "admin")
            .WithEnvironment("DISABLE_2FA", "true")
            .WithEnvironment("ENVIRONMENT", "dev")
            .WithEnvironment("NOTIFICATIONS_DISABLED", "True")
            .WithEnvironment("DEBUG", "True");
    }

    public static IResourceBuilder<T> WithReference<T>(this IResourceBuilder<T> maykin, IResourceBuilder<PostgresDatabaseResource> database) where T : MaykinResource
    {
        var postgres = database.Resource.Parent;

        return maykin
            .WithEnvironment("DB_USER", postgres.UserNameReference)
            .WithEnvironment("DB_HOST", postgres.Host)
            .WithEnvironment("DB_NAME", database.Resource.DatabaseName)
            .WithEnvironment("DB_PASSWORD", database.Resource.Parent.PasswordParameter);
    }

    public static IResourceBuilder<T> WithReference<T>(this IResourceBuilder<T> maykin, IResourceBuilder<RedisResource> redis) where T : MaykinResource
    {
        var cacheDb = redis.CreateCacheConnectionString();
        var celeryDb = redis.CreateCacheConnectionString();
        var celeryDbFix = ReferenceExpression.Create($"redis://{celeryDb}");

        return maykin
            .WithEnvironment("CACHE_DEFAULT", cacheDb)
            .WithEnvironment("CACHE_AXES", cacheDb)
            .WithEnvironment("CELERY_BROKER_URL", celeryDbFix)
            .WithEnvironment("CELERY_RESULT_BACKEND", celeryDbFix);
    }

    public static IResourceBuilder<T> WithTemplateFixtures<T>(this IResourceBuilder<T> maykin, string sourceFolder) where T : MaykinResource
    {
        return maykin.WithArgs("-c", """
        mkdir -p /app/fixtures &&
        for f in /app/templates/*.json; do
            envsubst < "$f" > "/app/fixtures/$(basename "$f")"
        done &&
        /start.sh
        """)
        .WithEntrypoint("sh")
        .WithBindMount(source: sourceFolder, target: "/app/templates", isReadOnly: true);
    }

    public static IResourceBuilder<ContainerResource> AddCeleryWorker<T>(this IResourceBuilder<T> maykin, string name) where T : MaykinResource
    {
        if (!maykin.Resource.TryGetContainerImageName(out var imageName)) throw new Exception();

        var result = maykin.ApplicationBuilder
            .AddContainer(name, imageName)  ;

        if (maykin.Resource.TryGetAnnotationsOfType<WaitAnnotation>(out var waits))
        {
            foreach (var item in waits)
            {
                result.WithAnnotation(item);
            }
        }

        if (maykin.Resource.TryGetAnnotationsOfType<CertificateTrustConfigurationCallbackAnnotation>(out var trusts))
        {
            foreach (var item in trusts)
            {
                result.WithAnnotation(item);
            }
        }

        if (maykin.Resource.TryGetEnvironmentVariables(out var env))
        {
            foreach (var item in env)
            {
                result.WithAnnotation(item);
            }
        }

        return result.WithContainerRuntimeArgs("--platform", "linux/amd64")
            .WithEntrypoint("/celery_worker.sh")
            .WithArgs();
    }

    private static ReferenceExpression CreateCacheConnectionString(this IResourceBuilder<RedisResource> redis)
    {
        var endpoint = redis.Resource.GetEndpoint("secondary");
        var hostAndPort = endpoint.Property(EndpointProperty.HostAndPort);
        var password = redis.Resource.PasswordParameter;
        var db = Interlocked.Increment(ref redisDbCounter).ToString();
        return password is null
            ? ReferenceExpression.Create($"{hostAndPort}/{db}")
            : ReferenceExpression.Create($":{password}@{hostAndPort}/{db}");
    }

    public static IResourceBuilder<ContainerResource> AddNginxProxy(this IResourceBuilder<MaykinResource> maykin, string name, int? port = null)
    {
        var nginx = maykin.ApplicationBuilder
            .AddContainer(name, "nginx")
            .WithHttpEndpoint(port: port, targetPort: 80)
            .WithEnvironment("TARGET_ADDRESS", maykin.GetEndpoint("http"))
            .WithBindMount(s_nginxPath, "/etc/nginx/templates/default.conf.template")
            .WaitFor(maykin)
            .WithParentRelationship(maykin);

        var endpoint = nginx.GetEndpoint("http");

        maykin
            .WithEnvironment("CSRF_TRUSTED_ORIGINS", () => $"http://localhost:{endpoint.Port}");

        return nginx;
    }
}
