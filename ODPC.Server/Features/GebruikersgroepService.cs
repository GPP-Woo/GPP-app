using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;
using ODPC.Features.Publicaties;

namespace ODPC.Features
{
    public interface IGebruikersgroepService
    {
        Task<IReadOnlyList<string>> GetWaardelijstUuidsAsync(Guid? gebruikersgroepUuid, CancellationToken token);
        Task<bool> IsGebruikersgroepGebruikerAsync(Guid gebruikersgroepUuid, CancellationToken token);
        Task<EigenaarGroep?> TryAndGetEigenaarGroepFromOdpcAsync(Guid publicatieUuid, CancellationToken token);
    }

    public class GebruikersgroepService(OdpcUser user, OdpcDbContext context) : IGebruikersgroepService
    {
        public async Task<IReadOnlyList<string>> GetWaardelijstUuidsAsync(Guid? gebruikersgroepUuid, CancellationToken token)
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

        public async Task<bool> IsGebruikersgroepGebruikerAsync(Guid gebruikersgroepUuid, CancellationToken token)
        {
            var lowerCaseUserId = user.Id?.ToLowerInvariant();

            if (string.IsNullOrEmpty(lowerCaseUserId)) return false;

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            return await context.GebruikersgroepGebruikers
                .AnyAsync(x => x.GebruikersgroepUuid == gebruikersgroepUuid &&
                               x.GebruikerId.ToLower() == lowerCaseUserId, token);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        }

        public async Task<EigenaarGroep?> TryAndGetEigenaarGroepFromOdpcAsync(Guid publicatieUuid, CancellationToken token)
        {
            // As we're now registering the publicatie <> EigenaarGroep (fka gebruikersgroep) relationship in the PUBLICATIEBANK
            // existing publicaties might not yet have set EigenaarGroep until updated again from ODPC.
            // If EigenaarGroep not set, try and get it's data from (legacy) ODPC GebruikersgroepPublicatie to prefill EigenaarGroep.
            // If no reference is found, e.g. it's an externally created publicatie, the EigenaarGroep will have to be selected manually in the interface.

            return await context.GebruikersgroepPublicatie
                .Where(x => x.PublicatieUuid == publicatieUuid)
                .Select(x => new EigenaarGroep
                {
                    identifier = x.GebruikersgroepUuid.ToString(),
                    weergaveNaam = x.Gebruikersgroep!.Naam
                })
                .FirstOrDefaultAsync(cancellationToken: token);
        }
    }
}
