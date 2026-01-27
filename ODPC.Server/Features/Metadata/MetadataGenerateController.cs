using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace ODPC.Features.Metadata
{
    [ApiController]
    public class MetadataGenerateController(IHttpClientFactory httpClientFactory, IConfiguration config) : ControllerBase
    {
        [HttpGet("api/v1/metadata/health")]
        public async Task<IActionResult> Health(CancellationToken token)
        {
            var baseUrl = config["WOO_HOO_BASE_URL"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return StatusCode(503);
            }

            try
            {
                using var client = httpClientFactory.CreateClient("WooHoo");
                client.BaseAddress = new Uri(baseUrl);

                using var response = await client.GetAsync("/health", token);

                return response.IsSuccessStatusCode ? Ok() : StatusCode(502);
            }
            catch
            {
                return StatusCode(502);
            }
        }

        [HttpPost("api/v1/metadata/generate/{documentUuid:guid}")]
        public async Task<IActionResult> Post(Guid documentUuid, CancellationToken token)
        {
            var baseUrl = config["WOO_HOO_BASE_URL"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return StatusCode(503, "Metadata generation service is not configured.");
            }

            using var client = httpClientFactory.CreateClient("WooHoo");
            client.BaseAddress = new Uri(baseUrl);

            var url = $"/api/v1/metadata/generate-from-publicatiebank?document_uuid={documentUuid}";

            using var response = await client.PostAsync(url, null, token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await response.Content.ReadFromJsonAsync<JsonNode>(token);

            return Ok(json);
        }
    }
}
