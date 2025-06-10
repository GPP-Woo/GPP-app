using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features.Publicaties.PublicatieVerwijderen
{
    [ApiController]
    public class PublicatieVerwijderenController(OdpcDbContext context, IOdrcClientFactory clientFactory, OdpcUser user) : ControllerBase
    {
        [HttpDelete("api/{version}/publicaties/{uuid:guid}")]
        public async Task<IActionResult> Delete(string version, Guid uuid, CancellationToken token)
        {
            // PUBLICATIEBANK

            using var client = clientFactory.Create("Publicatie verwijderen");

            var url = $"/api/{version}/publicaties/{uuid}";

            // publicatie ophalen
            using var getResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!getResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await getResponse.Content.ReadFromJsonAsync<Publicatie>(token);

            if (json?.Eigenaar?.identifier != user.Id)
            {
                return NotFound();
            }

            // publicatie verwijderen
            using var deleteResponse = await client.DeleteAsync(url, token);

            if (!deleteResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            // ODPC

            await context.GebruikersgroepPublicatie
                .Where(x => x.PublicatieUuid == uuid)
                .ExecuteDeleteAsync(token);

            return StatusCode(204);
        }
    }
}
