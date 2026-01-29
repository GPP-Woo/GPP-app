using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ODPC.Features.Metadata
{
    [ApiController]
    public class MetadataGenerateController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<MetadataGenerateController> logger) : ControllerBase
    {
        [HttpGet("api/v1/metadata/health")]
        [AllowAnonymous]
        public async Task<IActionResult> Health(CancellationToken token)
        {
            var baseUrl = config["WOO_HOO_BASE_URL"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogWarning("WOO_HOO_BASE_URL not configured");
                return StatusCode(503);
            }

            try
            {
                var timeoutSeconds = int.TryParse(config["WOO_HOO_HEALTH_TIMEOUT_SECONDS"], out var parsed) ? parsed : 30;

                using var client = httpClientFactory.CreateClient("WooHoo");
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                using var response = await client.GetAsync("/health", token);

                return response.IsSuccessStatusCode ? Ok() : StatusCode(502);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed");
                return StatusCode(502);
            }
        }

        [HttpPost("api/v1/metadata/generate/{documentUuid:guid}")]
        public async Task<IActionResult> Post(Guid documentUuid, CancellationToken token)
        {
            logger.LogInformation("Generating metadata for document {DocumentUuid}", documentUuid);

            var wooHooUrl = config["WOO_HOO_BASE_URL"];
            if (string.IsNullOrWhiteSpace(wooHooUrl))
            {
                logger.LogError("WOO_HOO_BASE_URL not configured");
                return StatusCode(503, "Metadata generation service is not configured.");
            }

            var odrcUrl = config["ODRC_BASE_URL"] ?? "http://localhost:8000";
            var odrcApiKey = config["ODRC_API_KEY"];

            try
            {
                // Step 1: Download PDF from ODRC
                logger.LogInformation("Downloading PDF from ODRC: {OdrcUrl}/api/v2/documenten/{DocumentUuid}/download", odrcUrl, documentUuid);

                using var odrcClient = httpClientFactory.CreateClient();
                if (!string.IsNullOrWhiteSpace(odrcApiKey))
                {
                    odrcClient.DefaultRequestHeaders.Add("Authorization", $"Token {odrcApiKey}");
                }

                // Add required audit headers for ODRC API
                odrcClient.DefaultRequestHeaders.Add("Audit-User-ID", "odpc-metadata-service");
                odrcClient.DefaultRequestHeaders.Add("Audit-User-Representation", "ODPC Metadata Generation Service");
                odrcClient.DefaultRequestHeaders.Add("Audit-Remarks", $"Downloading document for metadata generation");

                var pdfResponse = await odrcClient.GetAsync($"{odrcUrl}/api/v2/documenten/{documentUuid}/download", token);

                if (!pdfResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to download PDF: {StatusCode}", pdfResponse.StatusCode);
                    return StatusCode(502, "Failed to download document from ODRC");
                }

                var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync(token);
                logger.LogInformation("Downloaded {Size} bytes", pdfBytes.Length);

                // Get filename from ODRC metadata
                var metaResponse = await odrcClient.GetAsync($"{odrcUrl}/api/v2/documenten/{documentUuid}", token);
                var metadata = await metaResponse.Content.ReadFromJsonAsync<JsonNode>(token);
                var filename = metadata?["bestandsnaam"]?.GetValue<string>() ?? "document.pdf";

                // Step 2: Upload PDF to woo-hoo for metadata generation
                logger.LogInformation("Uploading PDF to woo-hoo: {WooHooUrl}", wooHooUrl);

                var generateTimeoutSeconds = int.TryParse(config["WOO_HOO_GENERATE_TIMEOUT_SECONDS"], out var parsedTimeout) ? parsedTimeout : 120;

                using var wooHooClient = httpClientFactory.CreateClient("WooHoo");
                wooHooClient.BaseAddress = new Uri(wooHooUrl);
                wooHooClient.Timeout = TimeSpan.FromSeconds(generateTimeoutSeconds);

                using var formContent = new MultipartFormDataContent
                {
                    { new ByteArrayContent(pdfBytes), "file", filename },
                };

                var wooHooResponse = await wooHooClient.PostAsync("/api/v1/metadata/generate-from-file", formContent, token);

                if (!wooHooResponse.IsSuccessStatusCode)
                {
                    var errorBody = await wooHooResponse.Content.ReadAsStringAsync(token);
                    logger.LogError("woo-hoo returned {StatusCode}: {Error}", wooHooResponse.StatusCode, errorBody);
                    return StatusCode(502, "Metadata generation failed");
                }

                var result = await wooHooResponse.Content.ReadFromJsonAsync<JsonNode>(token);
                logger.LogInformation("Successfully generated metadata");

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating metadata for document {DocumentUuid}", documentUuid);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
