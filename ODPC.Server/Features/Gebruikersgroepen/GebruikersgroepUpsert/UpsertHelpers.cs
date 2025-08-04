using ODPC.Data;
using ODPC.Data.Entities;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepUpsert
{
    public class UpsertHelpers
    {
        //voeg de nieuwe set waardelijsten toe aan deze groep
        public static void AddWaardelijstenToGroep(List<string> gekoppeldeWaardelijsten, Data.Entities.Gebruikersgroep groep, OdpcDbContext context)
        {
            context.GebruikersgroepWaardelijsten
                .AddRange(gekoppeldeWaardelijsten
                    .Select(x => new GebruikersgroepWaardelijst { Gebruikersgroep = groep, WaardelijstId = x }));
        }

        //voeg de nieuwe set gebruikers toe aan deze groep
        public static void AddGebruikersToGroep(List<string> gekoppeldeGebruikers, Data.Entities.Gebruikersgroep groep, OdpcDbContext context)
        {
            context.GebruikersgroepGebruikers
                .AddRange(gekoppeldeGebruikers
                    .Select(x => new GebruikersgroepGebruiker { Gebruikersgroep = groep, GebruikerId = x }));
        }
    }
}
