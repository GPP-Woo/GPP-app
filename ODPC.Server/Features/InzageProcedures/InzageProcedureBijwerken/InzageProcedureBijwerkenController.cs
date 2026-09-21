using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Features.Publicaties;

namespace ODPC.Features.InzageProcedures.InzageProcedureBijwerken
{
    [ApiController]
    public class InzageProcedureBijwerkenController(
        IOdrcClientFactory clientFactory,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpPut("api/{version}/inzageprocedure/{uuid:guid}")]
        public async Task<IActionResult> Put(string version, Guid uuid, InzageProcedure inzageProcedure, CancellationToken token)
        {
            using var client = clientFactory.Create("Inzageprocedure bijwerken");

            var url = $"/api/{version}/inzageprocedure/{uuid}";

            // bestaande inzageprocedure ophalen: de daaraan gekoppelde publicatie is leidend voor de autorisatiecheck,
            // niet het publicatieveld in de binnenkomende request, dat mogelijk gemanipuleerd is
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

            // eenmaal gekoppeld aan een publicatie is de inzage-procedure-autorisatie niet meer leidend bij bijwerken
            // de gebruiker moet wel lid zijn van de eigenaar-groep van de publicatie
            var isGebruikersgroepGebruiker = Guid.TryParse(publicatie?.EigenaarGroep?.identifier, out var eigenaarGroepIdentifier)
                && await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(eigenaarGroepIdentifier, token);

            if (!isGebruikersgroepGebruiker)
            {
                return NotFound();
            }

            // inzage-procedure bijwerken

            using var putResponse = await client.PutAsJsonAsync(url, inzageProcedure, token);

            putResponse.EnsureSuccessStatusCode();

            var viewModel = await putResponse.Content.ReadFromJsonAsync<InzageProcedure>(token);

            return viewModel == null ? NotFound() : Ok(viewModel);
        }
    }
}
