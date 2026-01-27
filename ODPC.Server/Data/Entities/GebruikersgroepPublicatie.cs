namespace ODPC.Data.Entities
{
    public class GebruikersgroepPublicatie
    {
        public Gebruikersgroep? Gebruikersgroep { get; set; }
        public Guid GebruikersgroepUuid { get; set; }
        public Guid PublicatieUuid { get; set; }
    }
}
