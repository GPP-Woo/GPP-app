using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepUpsert.GebruikersgroepAanmaken
{
    [ApiController]
    [Authorize(AdminPolicy.Name)]
    public class GebruikersgroepAanmakenController(OdpcDbContext context) : ControllerBase
    {
        private readonly OdpcDbContext _context = context;

        /// <summary>
        /// Gebruikerssroep aanmaken
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        [HttpPost("api/gebruikersgroepen")]
        public async Task<IActionResult> Post([FromBody] GebruikersgroepUpsertModel model, CancellationToken token)
        {
            try
            {
                var groep = new Data.Entities.Gebruikersgroep { Naam = model.Naam, Omschrijving = model.Omschrijving };
                _context.Gebruikersgroepen.Add(groep);

                UpsertHelpers.AddWaardelijstenToGroep(model.GekoppeldeWaardelijsten, groep, _context);
                UpsertHelpers.AddGebruikersToGroep(model.GekoppeldeGebruikers, groep, _context);

                await _context.SaveChangesAsync(token);

                return Ok(GebruikersgroepDetailsModel.MapEntityToViewModel(groep));
            }
            catch (DbUpdateException ex) when (ex.IsDuplicateException())
            {
                return Conflict(new { Message = "Naam bestaat al" });
            }
        }

    }
}
