using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Data;
using ODPC.Data.Entities;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepUpsert
{
    public class UpsertHelpers
    {
        // a gebruikersgroep must have at least one organisatie and one informatiecategorie linked
        public static bool ValidateGekoppeldeWaardelijsten(
            List<string>? gekoppeldeWaardelijsten,
            IReadOnlyDictionary<string, WaardelijstItem> waardelijsten,
            ModelStateDictionary modelState)
        {
            var gekoppeldeCategorieen = (gekoppeldeWaardelijsten ?? [])
                .Where(waardelijsten.ContainsKey)
                .Select(x => waardelijsten[x].Categorie)
                .ToHashSet();

            var isValid = true;

            if (!gekoppeldeCategorieen.Contains(WaardelijstCategorieen.Organisatie))
            {
                modelState.AddModelError(nameof(GebruikersgroepUpsertModel.GekoppeldeWaardelijsten), "Selecteer minimaal één organisatie");
                isValid = false;
            }

            if (!gekoppeldeCategorieen.Contains(WaardelijstCategorieen.Informatiecategorie))
            {
                modelState.AddModelError(nameof(GebruikersgroepUpsertModel.GekoppeldeWaardelijsten), "Selecteer minimaal één informatiecategorie");
                isValid = false;
            }

            return isValid;
        }

        //voeg de nieuwe set waardelijsten toe aan deze groep
        public static void AddWaardelijstenToGroep(List<string>? gekoppeldeWaardelijsten, Gebruikersgroep groep, OdpcDbContext context)
        {
            context.GebruikersgroepWaardelijsten
                .AddRange((gekoppeldeWaardelijsten ?? [])
                    .Select(x => new GebruikersgroepWaardelijst { Gebruikersgroep = groep, WaardelijstId = x }));
        }

        //voeg de nieuwe set gebruikers toe aan deze groep
        public static async Task AddGebruikersToGroep(List<string>? gekoppeldeGebruikers, Gebruikersgroep groep, OdpcDbContext context, CancellationToken token)
        {
            gekoppeldeGebruikers ??= [];

            //zorg eerst dat alle gebruikers bestaan in de Gebruikers tabel (FK constraint)
            var bestaandeGebruikerIds = (await context.Gebruikers
                .Where(g => gekoppeldeGebruikers.Contains(g.GebruikerId))
                .Select(g => g.GebruikerId)
                .ToListAsync(token)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var nieuweGebruikers = gekoppeldeGebruikers
                .Where(id => !bestaandeGebruikerIds.Contains(id))
                .Select(id => new Gebruiker { GebruikerId = id });

            context.Gebruikers.AddRange(nieuweGebruikers);

            context.GebruikersgroepGebruikers
                .AddRange(gekoppeldeGebruikers
                    .Select(x => new GebruikersgroepGebruiker { Gebruikersgroep = groep, GebruikerId = x }));
        }
    }
}
