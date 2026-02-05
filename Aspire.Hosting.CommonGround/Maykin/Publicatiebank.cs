using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.CommonGround.Maykin;

public class PublicatiebankResource(string name, string? entrypoint = null) : MaykinResource(name, entrypoint);

public static class PublicatiebankExtensions
{
    public static IResourceBuilder<PublicatiebankResource> AddPublicatiebank(this IDistributedApplicationBuilder builder, string name, string tag = "latest", int? port = null)
    {
        return builder
            .AddResource(new PublicatiebankResource(name))
            .WithImage("gpp-woo/gpp-publicatiebank", tag)
            .WithImageRegistry("ghcr.io")
            .WithMaykinDefaults(port)
            .WithEnvironment("DJANGO_SETTINGS_MODULE", "woo_publications.conf.docker")
            .WithEnvironment("ODRC_SUPERUSER_USERNAME", "admin")
            .WithEnvironment("ODRC_SUPERUSER_EMAIL", "admin@localhost")
            ;
    }
}
