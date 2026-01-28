using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;

namespace ODPC.Features.Publicaties.PublicatieDetails
{
    [ApiController]
    public class PublicatieDetailsController(
        IOdrcClientFactory clientFactory,
        OdpcUser user,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpGet("api/{version}/publicaties/{uuid:guid}")]
        public async Task<IActionResult> Put(string version, Guid uuid, CancellationToken token)
        {
            // PUBLICATIEBANK

            using var client = clientFactory.Create("Publicatie ophalen");

            var url = $"/api/{version}/publicaties/{uuid}";

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await response.Content.ReadFromJsonAsync<Publicatie>(token);

            if (json == null)
            {
                return NotFound();
            }

            json.EigenaarGroep ??= await gebruikersgroepService.TryAndGetEigenaarGroepFromOdpcAsync(uuid, token);

            var isGebruikersgroepGebruiker = Guid.TryParse(json.EigenaarGroep?.identifier, out var identifier)
                && await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(identifier, token);

            var isEigenaarZonderEigenaarGroep = json.EigenaarGroep == null
                && json.Eigenaar?.identifier?.ToLowerInvariant() == user.Id?.ToLowerInvariant();

            return isGebruikersgroepGebruiker || isEigenaarZonderEigenaarGroep
                ? Ok(json)
                : NotFound();
        }
    }
}
