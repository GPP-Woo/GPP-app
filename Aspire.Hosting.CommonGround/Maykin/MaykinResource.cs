using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.CommonGround.Maykin;

public abstract class MaykinResource(string name, string? entrypoint = null) : ContainerResource(name, entrypoint);

public static class MaykinExtensions
{
    private static readonly string s_executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("could not get executing directory");
    private static readonly string s_nginxPath = Path.Combine(s_executionPath, "nginx", "templates", "default.conf.template");

    public static IResourceBuilder<T> WithMaykinDefaults<T>(this IResourceBuilder<T> maykin, int? port = null) where T : MaykinResource
    {
        maykin = maykin.WithHttpEndpoint(port: port, targetPort: 8000);

        var endpoint = maykin.GetEndpoint("http");

        return maykin
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
            .WithEnvironment("NOTIFICATIONS_DISABLED", "True");
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
        var endpoint = redis.Resource.GetEndpoint("secondary");
        var hostAndPort = endpoint.Property(EndpointProperty.HostAndPort);
        var password = redis.Resource.PasswordParameter;

        var cacheUrl = password is null
            ? ReferenceExpression.Create($"{hostAndPort}/0")
            : ReferenceExpression.Create($":{password}@{hostAndPort}/0");

        return maykin
            .WithEnvironment("CACHE_DEFAULT", cacheUrl)
            .WithEnvironment("CACHE_AXES", cacheUrl);
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
