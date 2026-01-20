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

            var json = await response.Content.ReadFromJsonAsync<OdpcPublicatie>(token);

            if (json == null)
            {
                return NotFound();
            }

            // ODPC
            if (json.EigenaarGroep?.identifier != null)
            {
                json.Gebruikersgroep = Guid.TryParse(json.EigenaarGroep?.identifier, out var identifier)
                    ? identifier
                    : null;
            }
            else
            {
                var gebruikersgroepPublicatie = await context.GebruikersgroepPublicatie
                    .SingleOrDefaultAsync(x => x.PublicatieUuid == uuid, cancellationToken: token);

                json.Gebruikersgroep = gebruikersgroepPublicatie?.GebruikersgroepUuid;
            }

            return json.Eigenaar?.identifier == user.Id ? Ok(json) : NotFound();
        }
    }
}
