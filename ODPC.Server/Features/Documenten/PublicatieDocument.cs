namespace ODPC.Features.Documenten
{
    public class PublicatieDocument
    {
        public Guid Uuid { get; set; }
        public string? Identifier { get; set; }
        public Guid Publicatie { get; set; }
        public required string OfficieleTitel { get; set; }
        public string? VerkorteTitel { get; set; }
        public string? Omschrijving { get; set; }
        public Eigenaar? Eigenaar { get; set; }
        public string? Publicatiestatus { get; set; }
        public DateOnly Creatiedatum { get; set; }
        public required string Bestandsnaam { get; set; }
        public required string Bestandsformaat { get; set; }
        public required double Bestandsomvang { get; set; }
        public List<Bestandsdeel>? Bestandsdelen { get; set; }
        public List<Identifier>? Kenmerken
        {
            get
            {
                return [
                    new Identifier { Kenmerk = "DocKenmerk 1", Bron = "Bron1" },
                    new Identifier { Kenmerk = "DocKenmerk 2", Bron = "Bron2" },
                    new Identifier { Kenmerk = "DocKenmerk 3", Bron = "Bron3" }
                ];
            }
        }
    }

    public class Eigenaar
    {
        public string? identifier { get; set; }
        public string? weergaveNaam { get; set; }
    }

    public class Bestandsdeel
    {
        public required string Url { get; set; }
        public required int Volgnummer { get; set; }
        public required double Omvang { get; set; }
    }

    public class Identifier
    {
        public string? Kenmerk { get; set; }
        public string? Bron { get; set; }
    }
}
