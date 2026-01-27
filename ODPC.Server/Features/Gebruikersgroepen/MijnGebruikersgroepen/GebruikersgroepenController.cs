using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features.Gebruikersgroepen.MijnGebruikersgroepen
{
    [ApiController]
    public class MijnGebruikersgroepenController(OdpcUser user, OdpcDbContext context) : ControllerBase
    {
        [HttpGet("api/mijn-gebruikersgroepen")]
        public IAsyncEnumerable<MijnGebruikersgroepModel> Get()
        {
            var lowerCaseId = user.Id?.ToLowerInvariant();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            var groepIds = context.GebruikersgroepGebruikers
                .Where(x => x.GebruikerId.ToLower() == lowerCaseId)
                .Select(x => x.GebruikersgroepUuid);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            return context.Gebruikersgroepen
                .Where(x => groepIds.Contains(x.Uuid))
                .OrderBy(groep => groep.Naam)
                .Select(groep => new MijnGebruikersgroepModel
                {
                    Naam = groep.Naam,
                    Uuid = groep.Uuid,
                    GekoppeldeWaardelijsten = groep.Waardelijsten.Select(x => x.WaardelijstId).AsEnumerable()
                }
                )
                .AsAsyncEnumerable();
        }
    }
}
