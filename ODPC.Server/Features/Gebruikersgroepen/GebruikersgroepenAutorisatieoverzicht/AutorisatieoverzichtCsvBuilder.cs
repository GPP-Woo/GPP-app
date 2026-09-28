using System.Text;
using ODPC.Apis.Odrc;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepenAutorisatieoverzicht
{
    public static class AutorisatieoverzichtCsvBuilder
    {
        private const char Delimiter = ';';

        private static readonly string[] s_headers =
        [
            "Naam", "Omschrijving", "Toegevoegde gebruikers", "Organisaties", "Informatiecategorieën", "Onderwerpen"
        ];

        public static byte[] BuildBytes(
            IEnumerable<GebruikersgroepDetailsModel> groepen,
            IReadOnlyDictionary<string, WaardelijstItem> waardelijsten)
        {
            var encoding = new UTF8Encoding(true);
            var csv = Build(groepen, waardelijsten);

            return [.. encoding.GetPreamble(), .. encoding.GetBytes(csv)];
        }

        public static string Build(
            IEnumerable<GebruikersgroepDetailsModel> groepen,
            IReadOnlyDictionary<string, WaardelijstItem> waardelijsten)
        {
            var sb = new StringBuilder();

            AppendRow(sb, s_headers);

            foreach (var groep in groepen)
            {
                var gebruikers = groep.GekoppeldeGebruikers
                    .Select(g => g.Naam ?? g.GebruikerId)
                    .OrderBy(naam => naam, StringComparer.OrdinalIgnoreCase);

                AppendRow(sb,
                [
                    groep.Naam,
                    groep.Omschrijving ?? "",
                    string.Join(", ", gebruikers),
                    string.Join(", ", NamenVoorCategorie(groep.GekoppeldeWaardelijsten, waardelijsten, WaardelijstCategorieen.Organisatie)),
                    string.Join(", ", NamenVoorCategorie(groep.GekoppeldeWaardelijsten, waardelijsten, WaardelijstCategorieen.Informatiecategorie)),
                    string.Join(", ", NamenVoorCategorie(groep.GekoppeldeWaardelijsten, waardelijsten, WaardelijstCategorieen.Onderwerp))
                ]);
            }

            return sb.ToString();
        }

        private static IEnumerable<string> NamenVoorCategorie(
            IEnumerable<string> waardelijstIds,
            IReadOnlyDictionary<string, WaardelijstItem> waardelijsten,
            string categorie) =>
            waardelijstIds
                .Select(id => waardelijsten.TryGetValue(id, out var item) ? item : null)
                .Where(item => item?.Categorie == categorie)
                .Select(item => item!.Naam)
                .OrderBy(naam => naam, StringComparer.OrdinalIgnoreCase);

        private static void AppendRow(StringBuilder sb, IReadOnlyList<string> velden)
        {
            sb.AppendJoin(Delimiter, velden.Select(EscapeVeld));
            sb.Append("\r\n");
        }

        private static string EscapeVeld(string? veld)
        {
            veld ??= "";

            var moetQuoten = veld.Contains(Delimiter) || veld.Contains('"') || veld.Contains('\n') || veld.Contains('\r');

            return moetQuoten ? $"\"{veld.Replace("\"", "\"\"")}\"" : veld;
        }
    }
}
