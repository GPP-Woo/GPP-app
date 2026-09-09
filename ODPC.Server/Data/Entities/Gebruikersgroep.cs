using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;

namespace ODPC.Data.Entities
{
    public class Gebruikersgroep
    {
        public Guid Uuid { get; set; }
        public required string Naam { get; set; }
        public string? Omschrijving { get; set; }
        public bool IsGeautoriseerdVoorInzageProcedure { get; set; }

        public ICollection<GebruikersgroepWaardelijst> Waardelijsten { get; set; } = [];
        public ICollection<GebruikersgroepGebruiker> GebruikersgroepGebruikers { get; set; } = [];
    }
}
