using ODPC.Data.Entities;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails
{
    public class GebruikersgroepDetailsModel
    {
        public Guid Uuid { get; set; }
        public required string Naam { get; set; }
        public string? Omschrijving { get; set; }

        //Id's van de waardelijsten die gebruikt mogen worden binnen deze gebruikersgroep
        public required IEnumerable<string> GekoppeldeWaardelijsten { get; set; }

        public required IEnumerable<GekoppeldeGebruikerModel> GekoppeldeGebruikers { get; set; }

        public static IQueryable<GebruikersgroepDetailsModel> MapToViewModel(IQueryable<Gebruikersgroep> groepen) =>
            groepen.Select(groep => new GebruikersgroepDetailsModel
            {
                Uuid = groep.Uuid,
                Naam = groep.Naam,
                Omschrijving = groep.Omschrijving,
                GekoppeldeWaardelijsten = groep.Waardelijsten.Select(x => x.WaardelijstId),
                GekoppeldeGebruikers = groep.GebruikersgroepGebruikers.Select(x => new GekoppeldeGebruikerModel
                {
                    GebruikerId = x.GebruikerId,
                    Naam = x.Gebruiker!.Naam,
                    LastLogin = x.Gebruiker!.LastLogin
                })
            });

        public class GekoppeldeGebruikerModel
        {
            public required string GebruikerId { get; set; }
            public string? Naam { get; set; }
            public DateTimeOffset? LastLogin { get; set; }
        }
    }
}
