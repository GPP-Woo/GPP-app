using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features.Publicaties.PublicatieDetails
{
    [ApiController]
    public class PublicatieDetailsController(OdpcDbContext context, IOdrcClientFactory clientFactory, OdpcUser user) : ControllerBase
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

            if (json.Eigenaar?.identifier != user.Id)
            {
                return NotFound();
            }

            // As we're now registering the publicatie <> EigenaarGroep (fka gebruikersgroep) relationship in the PUBLICATIEBANK
            // existing publicaties might not yet have set EigenaarGroep until updated again from ODPC.
            // If EigenaarGroep not set, try and get it's data from (legacy) ODPC GebruikersgroepPublicatie to prefill EigenaarGroep.
            // If no reference is found, e.g. it's an externally created publicatie, the EigenaarGroep will have to be selected manually in the interface.
            if (json.EigenaarGroep == null)
            {
                var gebruikersgroepPublicatie = await context.GebruikersgroepPublicatie
                    .Include(x => x.Gebruikersgroep)
                    .SingleOrDefaultAsync(x => x.PublicatieUuid == uuid, cancellationToken: token);

                json.EigenaarGroep = gebruikersgroepPublicatie != null
                    ? new EigenaarGroep
                    {
                        identifier = gebruikersgroepPublicatie.GebruikersgroepUuid.ToString(),
                        weergaveNaam = gebruikersgroepPublicatie.Gebruikersgroep?.Naam ?? "Onbekend"
                    } : null;
            }

            return Ok(json);
        }
    }
}
