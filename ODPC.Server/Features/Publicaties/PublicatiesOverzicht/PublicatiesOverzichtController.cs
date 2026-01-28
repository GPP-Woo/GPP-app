using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;

namespace ODPC.Features.Publicaties.PublicatiesOverzicht
{
    [ApiController]
    public class PublicatiesOverzichtController(
        IOdrcClientFactory clientFactory,
        OdpcUser user,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
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
            [FromQuery] string? informatieCategorieen = "",
            [FromQuery] string? onderwerpen = "",
            [FromQuery] string? publicatiestatus = "",
            [FromQuery] string? eigenaarGroep = null)
        {
            using var client = clientFactory.Create("Publicaties ophalen");

            var parameters = new Dictionary<string, string?>
            {
                { "page", page },
                { "sorteer", sorteer },
                { "search", search },
                { "registratiedatumVanaf", registratiedatumVanaf },
                { "registratiedatumTot", registratiedatumTot },                
                { "informatieCategorieen", informatieCategorieen },
                { "onderwerpen", onderwerpen },
                { "publicatiestatus", publicatiestatus },
                { "pageSize", "10" }
            };

            var emptyResult = new PagedResponseModel<JsonNode> { Results = [], Count = 0 };

            if (Guid.TryParse(eigenaarGroep, out var identifier))
            {
                var isGebruikersgroepGebruiker =
                    await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(identifier, token);

                if (isGebruikersgroepGebruiker)
                {
                    parameters.Add("eigenaarGroep", identifier.ToString());
                }
                else
                {
                    return Ok(emptyResult);
                }
            }
            else if (!string.IsNullOrEmpty(eigenaarGroep))
            {
                return Ok(emptyResult);
            }
            else
            {
                parameters.Add("eigenaar", WebUtility.UrlEncode(user.Id));
            }

            // publicaties ophalen uit het ODRC

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
