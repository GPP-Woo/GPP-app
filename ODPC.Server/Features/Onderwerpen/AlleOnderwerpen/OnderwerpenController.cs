using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Onderwerpen.AlleOnderwerpen
{
    [ApiController]
    public class OnderwerpenController(IOdrcClientFactory clientFactory) : ControllerBase
    {
        [HttpGet("api/{version}/onderwerpen")]
        public async Task<IActionResult> Get(string version, CancellationToken token, [FromQuery] string? page = "1")
        {
            // onderwerpen ophalen uit het ODRC
            using var client = clientFactory.Create("Onderwerpen ophalen");
            var url = $"/api/{version}/onderwerpen?page={page}&publicatiestatus=concept,gepubliceerd";

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await response.Content.ReadFromJsonAsync<PagedResponseModel<JsonNode>>(token);

            return Ok(json);
        }
    }
}
