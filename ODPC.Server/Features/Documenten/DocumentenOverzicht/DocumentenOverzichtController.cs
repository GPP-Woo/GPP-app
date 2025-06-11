using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;

namespace ODPC.Features.Documenten.DocumentenOverzicht
{
    [ApiController]
    public class DocumentenOverzichtController(IOdrcClientFactory clientFactory) : ControllerBase
    {
        [HttpGet("api/{version}/documenten")]
        public async Task<IActionResult> Get(
            string version,
            [FromQuery] string publicatie,
            OdpcUser user,
            CancellationToken token,
            [FromQuery] string? page = "1")
        {
            // documenten ophalen uit het ODRC
            using var client = clientFactory.Create("Documenten ophalen");

            var url = $"/api/{version}/documenten?publicatie={publicatie}&eigenaar={WebUtility.UrlEncode(user.Id)}&page={page}";

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
