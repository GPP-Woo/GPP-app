using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Features.Publicaties;

namespace ODPC.Features.InzageProcedures.InzageProcedureOverzicht
{
    [ApiController]
    public class InzageProcedureOverzichtController(
        IOdrcClientFactory clientFactory,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpGet("api/{version}/inzageprocedure")]
        public async Task<IActionResult> Get(
            string version,
            [FromQuery] Guid publicatie,
            OdpcUser user,
            CancellationToken token)
        {
            var emptyResult = new PagedResponseModel<InzageProcedure> { Results = [], Count = 0 };

            using var client = clientFactory.Create("Inzageprocedure ophalen");

            // publicatie ophalen

            using var publicatieResponse = await client.GetAsync(
                $"/api/{version}/publicaties/{publicatie}", HttpCompletionOption.ResponseHeadersRead, token);

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

            // gebruiker mag de inzage-procedure raadplegen als:
            // a. in groep van publicatie zit
            // b. en/of eigenaar van publicatie is

            var isGebruikersgroepGebruiker = Guid.TryParse(publicatieJson.EigenaarGroep?.identifier, out var identifier)
                && await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(identifier, token);

            if (!isGebruikersgroepGebruiker
                && publicatieJson.Eigenaar?.identifier?.ToLowerInvariant() != user.Id?.ToLowerInvariant())
            {
                return Ok(emptyResult);
            }

            // inzage-procedure ophalen: een publicatie heeft er ten hoogste één

            var url = $"/api/{version}/inzageprocedure?publicatie={publicatie}";

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await response.Content.ReadFromJsonAsync<PagedResponseModel<InzageProcedure>>(token);

            var inzageProcedure = json?.Results.SingleOrDefault();

            return Ok(inzageProcedure != null
                ? new PagedResponseModel<InzageProcedure> { Results = [inzageProcedure], Count = 1 }
                : emptyResult);
        }
    }
}
