namespace ODPC.Data.Entities
{
    public class Gebruiker
    {
        public required string GebruikerId { get; set; }
        public string? Naam { get; set; }
        public DateTimeOffset? LastLogin { get; set; }
    }
}
