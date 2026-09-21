using System.Text.Json.Serialization;

namespace ODPC.Features.InzageProcedures
{
    public class InzageProcedure
    {
        public Guid Uuid { get; set; }
        public Guid Publicatie { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UrlBekendmaking { get; set; }
        public required string Toelichting { get; set; }
        public required string BeschikbaarRechtsmiddel { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UrlReactieformulier { get; set; }
        public DateOnly DatumBeginInzagetermijn { get; set; }
        public DateOnly DatumEindeInzagetermijn { get; set; }
        public bool AutomatischIntrekken { get; set; }
    }
}
