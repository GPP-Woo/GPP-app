using System.Text.Json.Serialization;

namespace ODPC.Features.Publicaties
{
    public class Publicatie
    {
        public Guid Uuid { get; set; }
        public string? Publisher { get; set; }
        public string? Verantwoordelijke { get; set; }
        public string? OfficieleTitel { get; set; }
        public string? VerkorteTitel { get; set; }
        public string? Omschrijving { get; set; }
        public Eigenaar? Eigenaar { get; set; }
        public EigenaarGroep? EigenaarGroep { get; set; }
        public string? Publicatiestatus { get; set; }
        public DateTimeOffset Registratiedatum { get; set; }
        public DateOnly? DatumBeginGeldigheid { get; set; }
        public DateOnly? DatumEindeGeldigheid { get; set; }
        public List<string>? InformatieCategorieen { get; set; }
        public List<string>? Onderwerpen { get; set; }
        public List<Identifier>? Kenmerken { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UrlPublicatieExtern { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BronBewaartermijn { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Selectiecategorie { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Archiefnominatie { get; set; }
        public string? Archiefactiedatum { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ToelichtingBewaartermijn { get; set; }
    }

    public class Eigenaar
    {
        public string? identifier { get; set; }
        public string? weergaveNaam { get; set; }
    }

    public class EigenaarGroep
    {
        public string? identifier { get; set; }
        public string? weergaveNaam { get; set; }
    }

    public class Identifier
    {
        public string? Kenmerk { get; set; }
        public string? Bron { get; set; }
    }
}
