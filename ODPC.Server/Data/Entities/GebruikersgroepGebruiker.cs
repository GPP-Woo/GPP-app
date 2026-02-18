namespace ODPC.Data.Entities
{
    public class GebruikersgroepGebruiker
    {
        public Guid GebruikersgroepUuid { get; set; }
        public required string GebruikerId { get; set; }
        public Gebruikersgroep? Gebruikersgroep { get; set; }
        public Gebruiker? Gebruiker { get; set; }
    }
}
