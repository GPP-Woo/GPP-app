using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Features.Publicaties;

namespace ODPC.Features.InzageProcedures.InzageProcedureAanmaken
{
    [ApiController]
    public class InzageProcedureAanmakenController(
        IOdrcClientFactory clientFactory,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpPost("api/{version}/inzageprocedure")]
        public async Task<IActionResult> Post(string version, InzageProcedure inzageProcedure, CancellationToken token)
        {
            using var client = clientFactory.Create("Inzageprocedure aanmaken");

            // publicatie ophalen om de eigenaar-groep van de publicatie te bepalen
            using var publicatieResponse = await client.GetAsync(
                $"/api/{version}/publicaties/{inzageProcedure.Publicatie}", HttpCompletionOption.ResponseHeadersRead, token);

            if (!publicatieResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var publicatie = await publicatieResponse.Content.ReadFromJsonAsync<Publicatie>(token);

            Guid? eigenaarGroepIdentifier = Guid.TryParse(publicatie?.EigenaarGroep?.identifier, out var identifier)
                ? identifier
                : null;

            // autorisatie via gebruikergroep voor inzage-procedure geldt alleen bij het (voor het eerst) koppelen
            if (!await gebruikersgroepService.IsGeautoriseerdVoorInzageProcedureAsync(eigenaarGroepIdentifier, token))
            {
                ModelState.AddModelError(nameof(InzageProcedure), "Gebruiker is niet geautoriseerd voor het koppelen van een Inzage-procedure");
                return BadRequest(ModelState);
            }

            // inzage-procedure registreren/koppelen

            var url = $"/api/{version}/inzageprocedure";

            using var response = await client.PostAsJsonAsync(url, inzageProcedure, token);

            response.EnsureSuccessStatusCode();

            var viewModel = await response.Content.ReadFromJsonAsync<InzageProcedure>(token);

            return viewModel == null ? NotFound() : Ok(viewModel);
        }
    }
}
