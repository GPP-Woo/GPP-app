using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.CommonGround.Maykin;

public class OpenZaakResource(string name, string? entrypoint = null) : MaykinResource(name, entrypoint);

public static class OpenZaakExtensions
{
    public static IResourceBuilder<OpenZaakResource> AddOpenZaak(this IDistributedApplicationBuilder builder, string name, string tag = "latest", int? port = null)
    {
        return builder
            .AddResource(new OpenZaakResource(name))
            .WithImage("openzaak/open-zaak", tag)
            .WithImageRegistry("docker.io")
            .WithMaykinDefaults(port)
            .WithEnvironment("DJANGO_SETTINGS_MODULE", "openzaak.conf.docker")
            .WithEnvironment("IMPORT_DOCUMENTEN_BASE_DIR", "/app/import-data")
            .WithEnvironment("IMPORT_DOCUMENTEN_BATCH_SIZE", "500")
            .WithEnvironment("OPENZAAK_SUPERUSER_USERNAME", "admin")
            .WithEnvironment("OPENZAAK_SUPERUSER_EMAIL", "admin@localhost")
            .WithEnvironment("SENDFILE_BACKEND", "django_sendfile.backends.simple")
            .WithVolume("openzaak-private-media", "/app/private-media")
            .WithVolume("openzaak-media", "/app/media")
            ;
    }
}
