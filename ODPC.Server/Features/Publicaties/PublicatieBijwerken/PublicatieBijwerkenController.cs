using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features.Publicaties.PublicatieBijwerken
{
    [ApiController]
    public class PublicatieBijwerkenController(
        OdpcDbContext context,
        IOdrcClientFactory clientFactory,
        IGebruikerWaardelijstItemsService waardelijstItemsService,
        OdpcUser user) : ControllerBase
    {
        [HttpPut("api/{version}/publicaties/{uuid:guid}")]
        public async Task<IActionResult> Put(string version, Guid uuid, OdpcPublicatie publicatie, CancellationToken token)
        {
            var gebruikersgroepWaardelijstUuids = await waardelijstItemsService.GetAsync(publicatie.Gebruikersgroep, token);

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

            if(publicatie.Gebruikersgroep == null)
            {
                ModelState.AddModelError(nameof(publicatie.Gebruikersgroep), "Publicatie is niet gekoppeld aan een gebruikergroep");
                return BadRequest(ModelState);
            }

            // update odpc -----
            await context.GebruikersgroepPublicatie
                .Where(x => x.PublicatieUuid == uuid)
                .ExecuteDeleteAsync(token);

            var row = new Data.Entities.GebruikersgroepPublicatie
            {
                GebruikersgroepUuid = (Guid)publicatie.Gebruikersgroep,
                PublicatieUuid = uuid
            };
            
            context.GebruikersgroepPublicatie.Add(row);
           
            await context.SaveChangesAsync(token);
            // -----

            using var client = clientFactory.Create("Publicatie bijwerken");

            var url = $"/api/{version}/publicaties/{uuid}";

            // publicatie ophalen
            using var getResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!getResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await getResponse.Content.ReadFromJsonAsync<OdpcPublicatie>(token);

            if (json?.Eigenaar?.identifier != user.Id)
            {
                return NotFound();
            }

            // publicatie bijwerken
            using var putResponse = await client.PutAsJsonAsync<Publicatie>(url, publicatie, token);

            putResponse.EnsureSuccessStatusCode();

            var viewModel = await putResponse.Content.ReadFromJsonAsync<OdpcPublicatie>(token);

            if (viewModel == null)
            {
                return NotFound();
            }

            viewModel.Gebruikersgroep = publicatie.Gebruikersgroep;

            return Ok(viewModel);
        }
    }
}
