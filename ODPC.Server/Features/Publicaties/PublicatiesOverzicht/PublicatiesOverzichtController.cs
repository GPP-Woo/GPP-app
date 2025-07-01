using Microsoft.AspNetCore.Mvc;
using ODPC.Authentication;
using ODPC.Apis.Odrc;
using System.Text.Json.Nodes;
using System.Net;

namespace ODPC.Features.Publicaties.PublicatiesOverzicht
{
    [ApiController]
    public class PublicatiesOverzichtController(IOdrcClientFactory clientFactory, OdpcUser user) : ControllerBase
    {
        [HttpGet("api/{version}/publicaties")]
        public async Task<IActionResult> Get(
            string version,
            CancellationToken token,
            [FromQuery] string? page = "1",
            [FromQuery] string? sorteer = "-registratiedatum",
            [FromQuery] string? search = "",
            [FromQuery] string? registratiedatumVanaf = "",
            [FromQuery] string? registratiedatumTot = "",
            [FromQuery] string? publishers = "",
            [FromQuery] string? informatieCategorieen = "",
            [FromQuery] string? onderwerpen = "")
        {
            // publicaties ophalen uit het ODRC
            using var client = clientFactory.Create("Publicaties ophalen");

            var parameters = new Dictionary<string, string?>
            {
                { "eigenaar", WebUtility.UrlEncode(user.Id) },
                { "page", page },
                { "sorteer", sorteer },
                { "search", search },
                { "registratiedatumVanaf", registratiedatumVanaf },
                { "registratiedatumTot", registratiedatumTot },
                { "publishers", publishers },
                { "informatieCategorieen", informatieCategorieen },
                { "onderwerpen", onderwerpen },
                { "pageSize", "10" }
            };

            var url = $"/api/{version}/publicaties?{UrlHelper.BuildQueryString(parameters)}";

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
