using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Data;

namespace ODPC.Features.Publicaties.PublicatieRegistreren
{
    [ApiController]
    public class PublicatieRegistrerenController(
        OdpcDbContext context,
        IOdrcClientFactory clientFactory,
        IGebruikerWaardelijstItemsService waardelijstItemsService) : ControllerBase
    {
        [HttpPost("api/{version}/publicaties")]
        public async Task<IActionResult> Post(string version, OdpcPublicatie publicatie, CancellationToken token)
        {
            var gebruikersgroepWaardelijstUuids = await waardelijstItemsService.GetAsync(publicatie.Gebruikersgroep, token);

            if (publicatie.Gebruikersgroep == null)
            {
                ModelState.AddModelError(nameof(publicatie.Gebruikersgroep), "Publicatie is niet gekoppeld aan een gebruikergroep");
                return BadRequest(ModelState);
            }

            if (publicatie.Publisher != null && !gebruikersgroepWaardelijstUuids.Contains(publicatie.Publisher))
            {
                ModelState.AddModelError(nameof(publicatie.Publisher), "Gebruiker is niet geautoriseerd voor deze organisatie");
                return BadRequest(ModelState);
            }

            if (publicatie.InformatieCategorieen != null && publicatie.InformatieCategorieen.Any(c => !gebruikersgroepWaardelijstUuids.Contains(c)))
            {
                ModelState.AddModelError(nameof(publicatie.InformatieCategorieen), "Gebruiker is niet geautoriseerd voor deze informatiecategorieën");
                return BadRequest(ModelState);
            }

            if (publicatie.Onderwerpen != null && publicatie.Onderwerpen.Any(c => !gebruikersgroepWaardelijstUuids.Contains(c)))
            {
                ModelState.AddModelError(nameof(publicatie.Onderwerpen), "Gebruiker is niet geautoriseerd voor deze onderwerpen");
                return BadRequest(ModelState);
            }

            // PUBLICATIEBANK

            using var client = clientFactory.Create("Publicatie registreren");

            var url = $"/api/{version}/publicaties";

            using var response = await client.PostAsJsonAsync<Publicatie>(url, publicatie, token);

            response.EnsureSuccessStatusCode();

            var viewModel = await response.Content.ReadFromJsonAsync<OdpcPublicatie>(token);

            if (viewModel == null)
            {
                return NotFound();
            }

            viewModel.Gebruikersgroep = publicatie.Gebruikersgroep;

            // ODPC

            context.GebruikersgroepPublicatie.Add
            (
                new Data.Entities.GebruikersgroepPublicatie
                {
                    GebruikersgroepUuid = (Guid)publicatie.Gebruikersgroep,
                    PublicatieUuid = viewModel.Uuid
                }
            );

            await context.SaveChangesAsync(token);

            return Ok(viewModel);
        }
    }
}
