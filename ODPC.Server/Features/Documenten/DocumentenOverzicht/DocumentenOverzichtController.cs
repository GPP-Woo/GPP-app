using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Features.Publicaties;

namespace ODPC.Features.Documenten.DocumentenOverzicht
{
    [ApiController]
    public class DocumentenOverzichtController(
        IOdrcClientFactory clientFactory,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpGet("api/{version}/documenten")]
        public async Task<IActionResult> Get(
            string version,
            [FromQuery] Guid publicatie,
            OdpcUser user,
            CancellationToken token,
            [FromQuery] string? page = "1")
        {
            var emptyResult = new PagedResponseModel<JsonNode> { Results = [], Count = 0 };

            // publicatie ophalen uit het ODRC

            using var publicatieClient = clientFactory.Create("Publicatie ophalen");

            using var publicatieResponse =
                await publicatieClient.GetAsync($"/api/{version}/publicaties/{publicatie}", HttpCompletionOption.ResponseHeadersRead, token);

            if (!publicatieResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var publicatieJson = await publicatieResponse.Content.ReadFromJsonAsync<Publicatie>(token);

            if (publicatieJson == null)
            {
                return Ok(emptyResult);
            }

            publicatieJson.EigenaarGroep ??=
                await gebruikersgroepService.TryAndGetEigenaarGroepFromOdpcAsync(publicatie, token);

            // gebruiker mag documenten raadplegen als:
            // a. in groep van publicatie zit
            // b. en/of eigenaar van publicatie is

            var isGebruikersgroepGebruiker = Guid.TryParse(publicatieJson.EigenaarGroep?.identifier, out var identifier)
                && await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(identifier, token);

            if (!isGebruikersgroepGebruiker
                && publicatieJson.Eigenaar?.identifier?.ToLowerInvariant() != user.Id?.ToLowerInvariant())
            {
                return Ok(emptyResult);
            }

            // documenten ophalen uit het ODRC

            using var client = clientFactory.Create("Documenten ophalen");

            var url = $"/api/{version}/documenten?publicatie={publicatie}&page={page}";

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
