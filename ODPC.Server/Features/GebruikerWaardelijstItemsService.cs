using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features
{
    public interface IGebruikerWaardelijstItemsService
    {
        Task<IReadOnlyList<string>> GetAsync(CancellationToken token);
    }

    public class GebruikerWaardelijstItemsService(OdpcUser user, OdpcDbContext context) : IGebruikerWaardelijstItemsService
    {
        public async Task<IReadOnlyList<string>> GetAsync(CancellationToken token)
        {
            var lowerCaseId = user.Id?.ToLowerInvariant();

            if (lowerCaseId == null) return [];

#pragma warning disable CA1862 // Needed by ef core: Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            var groepIds = context.GebruikersgroepGebruikers
                .Where(x => x.GebruikerId.ToLower() == lowerCaseId)
                .Select(x => x.GebruikersgroepUuid);
#pragma warning restore CA1862 // Needed by ef core: Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            return await context.GebruikersgroepWaardelijsten
                .Where(x => groepIds.Contains(x.GebruikersgroepUuid))
                .Select(x => x.WaardelijstId)
                .Distinct()
                .ToListAsync(token);
        }
    }
}
