using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Features.Publicaties;

namespace ODPC.Features.InzageProcedures.InzageProcedureVerwijderen
{
    [ApiController]
    public class InzageProcedureVerwijderenController(
        IOdrcClientFactory clientFactory,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpDelete("api/{version}/inzageprocedure/{uuid:guid}")]
        public async Task<IActionResult> Delete(string version, Guid uuid, CancellationToken token)
        {
            using var client = clientFactory.Create("Inzageprocedure verwijderen");

            var url = $"/api/{version}/inzageprocedure/{uuid}";

            // bestaande inzageprocedure ophalen: de daaraan gekoppelde publicatie is leidend voor de autorisatiecheck
            using var getResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!getResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var bestaandeInzageProcedure = await getResponse.Content.ReadFromJsonAsync<InzageProcedure>(token);

            if (bestaandeInzageProcedure == null)
            {
                return NotFound();
            }

            // publicatie ophalen

            using var publicatieResponse = await client.GetAsync(
                $"/api/{version}/publicaties/{bestaandeInzageProcedure.Publicatie}", HttpCompletionOption.ResponseHeadersRead, token);

            if (!publicatieResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var publicatie = await publicatieResponse.Content.ReadFromJsonAsync<Publicatie>(token);

            // eenmaal gekoppeld aan een publicatie is de inzage-procedure-autorisatie niet meer leidend bij verwijderen
            // de gebruiker moet wel lid zijn van de eigenaar-groep van de publicatie
            var isGebruikersgroepGebruiker = Guid.TryParse(publicatie?.EigenaarGroep?.identifier, out var eigenaarGroepIdentifier)
                && await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(eigenaarGroepIdentifier, token);

            if (!isGebruikersgroepGebruiker)
            {
                return NotFound();
            }

            // inzage-procedure verwijderen

            using var deleteResponse = await client.DeleteAsync(url, token);

            return !deleteResponse.IsSuccessStatusCode ? StatusCode(502) : StatusCode(204);
        }
    }
}
