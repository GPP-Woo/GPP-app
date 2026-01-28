using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Data;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails;
using Serilog;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepUpsert.GebruikersgroepBijwerken
{
    [ApiController]
    [Authorize(AdminPolicy.Name)]
    public class GebruikersgroepBijwerkenController(OdpcDbContext context, IOdrcClientFactory clientFactory) : ControllerBase
    {
        private readonly OdpcDbContext _context = context;

        /// <summary>
        /// Gebruikersgroep bijwerken
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        [HttpPut("api/gebruikersgroepen/{uuid:guid}")]
        public async Task<IActionResult> Put(
            Guid uuid,
            [FromBody] GebruikersgroepUpsertModel model,
            ILogger<GebruikersgroepBijwerkenController> logger,
            CancellationToken token)
        {
            var groep = await _context.Gebruikersgroepen.SingleOrDefaultAsync(x => x.Uuid == uuid, cancellationToken: token);

            if (groep == null) return NotFound();

            try
            {
                //update groep waardes en save voor duplicate check
                groep.Naam = model.Naam;
                groep.Omschrijving = model.Omschrijving;

                await _context.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (ex.IsDuplicateException())
            {
                return Conflict(new { Message = "Naam bestaat al" });
            }

            //verwijder bestaande waardelijsten voor deze groep
            await _context
               .GebruikersgroepWaardelijsten
               .Where(x => x.GebruikersgroepUuid == groep.Uuid)
               .ExecuteDeleteAsync(cancellationToken: token);

            //verwijder bestaande gebruikers voor deze groep
            await _context
                .GebruikersgroepGebruikers
                .Where(x => x.GebruikersgroepUuid == groep.Uuid)
                .ExecuteDeleteAsync(cancellationToken: token);

            //voeg de nieuwe selectie waardelijsten en gebruikers toe aan deze groep
            UpsertHelpers.AddWaardelijstenToGroep(model.GekoppeldeWaardelijsten, groep, _context);
            UpsertHelpers.AddGebruikersToGroep(model.GekoppeldeGebruikers, groep, _context);

            await _context.SaveChangesAsync(token);

            //sync naam naar gerelateerde organisatie-eenheid in de publicatiebank
            try
            {
                using var client = clientFactory.Create("Organisatie-eenheid bijwerken");

                var url = $"/api/v2/accounts/organisatie-eenheden/{uuid}";

                using var response = await client.PutAsJsonAsync(url, new { naam = model.Naam }, token);

                //organisatie-eenheid bestaat mogelijk (nog) niet in publicatiebank
                //negeer 404, log andere excepties
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Onverwachte statuscode bij updaten organisatie-eenheid in publicatiebank.");
            }

            return Ok(GebruikersgroepDetailsModel.MapEntityToViewModel(groep));
        }
    }
}
