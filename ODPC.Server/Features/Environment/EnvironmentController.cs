using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ODPC.Features.Environment
{
    [ApiController]
    [Route("api/environment")]
    [AllowAnonymous]
    public class EnvironmentController() : ControllerBase
    {
        private record VersionInfo(string? SemanticVersion, string? GitSha);

        private static readonly VersionInfo s_versionInfo = GetVersionInfo();

        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok(s_versionInfo);
        }

        private static VersionInfo GetVersionInfo()
        {
            var parts = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?.Split('+') ?? [];
            return new VersionInfo(parts.ElementAtOrDefault(0), parts.ElementAtOrDefault(1));
        }
    }
}
