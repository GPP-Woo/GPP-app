using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features
{
    public interface IGebruikerWaardelijstItemsService
    {
        Task<IReadOnlyList<string>> GetAsync(Guid? gebruikersgroepUuid, CancellationToken token);
    }

    public class GebruikerWaardelijstItemsService(OdpcUser user, OdpcDbContext context) : IGebruikerWaardelijstItemsService
    {
        public async Task<IReadOnlyList<string>> GetAsync(Guid? gebruikersgroepUuid, CancellationToken token)
        {
            var lowerCaseId = user.Id?.ToLowerInvariant();

            if (lowerCaseId == null || gebruikersgroepUuid == null) return [];

#pragma warning disable CA1862 // Needed by ef core: Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            var count = await context.GebruikersgroepGebruikers
                .CountAsync(x => x.GebruikerId.ToLower() == lowerCaseId && x.GebruikersgroepUuid == gebruikersgroepUuid, token);
#pragma warning restore CA1862 // Needed by ef core: Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            return count != 1
                ? []
                : await context.GebruikersgroepWaardelijsten
                    .Where(x => x.GebruikersgroepUuid == gebruikersgroepUuid)
                    .Select(x => x.WaardelijstId)
                    .Distinct()
                    .ToListAsync(token);
        }
    }
}
