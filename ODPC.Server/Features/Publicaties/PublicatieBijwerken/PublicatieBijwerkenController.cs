using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;

namespace ODPC.Features.Publicaties.PublicatieBijwerken
{
    [ApiController]
    public class PublicatieBijwerkenController(
        IOdrcClientFactory clientFactory,
        IGebruikerWaardelijstItemsService waardelijstItemsService,
        OdpcUser user) : ControllerBase
    {
        [HttpPut("api/{version}/publicaties/{uuid:guid}")]
        public async Task<IActionResult> Put(string version, Guid uuid, OdpcPublicatie publicatie, CancellationToken token)
        {
            var waardelijstItems = await waardelijstItemsService.GetAsync(token);

            if (publicatie.Publisher != null && !waardelijstItems.Contains(publicatie.Publisher))
            {
                ModelState.AddModelError(nameof(publicatie.Publisher), "Gebruiker is niet geautoriseerd voor deze organisatie");
                return BadRequest(ModelState);
            }

            if (publicatie.InformatieCategorieen != null && publicatie.InformatieCategorieen.Any(c => !waardelijstItems.Contains(c)))
            {
                ModelState.AddModelError(nameof(publicatie.InformatieCategorieen), "Gebruiker is niet geautoriseerd voor deze informatiecategorieën");
                return BadRequest(ModelState);
            }

            using var client = clientFactory.Create("Publicatie bijwerken");

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

            // publicatie bijwerken
            using var putResponse = await client.PutAsJsonAsync<Publicatie>(url, publicatie, token);

            putResponse.EnsureSuccessStatusCode();

            var viewModel = await putResponse.Content.ReadFromJsonAsync<Publicatie>(token);

            return Ok(viewModel);
        }
    }
}
