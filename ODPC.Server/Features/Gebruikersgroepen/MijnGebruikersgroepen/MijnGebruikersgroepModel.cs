namespace ODPC.Features.Gebruikersgroepen.MijnGebruikersgroepen
{
    public class MijnGebruikersgroepModel
    {
        public Guid Uuid { get; set; }
        public required string Naam { get; set; }
        public required IEnumerable<string> GekoppeldeWaardelijsten { get; set; }
    }
}
