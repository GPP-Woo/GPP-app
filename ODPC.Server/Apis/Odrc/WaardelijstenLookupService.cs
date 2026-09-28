using System.Text.Json.Nodes;
using ODPC.Features;

namespace ODPC.Apis.Odrc
{
    public static class WaardelijstCategorieen
    {
        public const string Organisatie = "Organisatie";
        public const string Informatiecategorie = "Informatiecategorie";
        public const string Onderwerp = "Onderwerp";
    }

    public interface IWaardelijstenLookupService
    {
        Task<IReadOnlyDictionary<string, WaardelijstItem>> GetAllAsync(string reden, CancellationToken token);
    }

    public class WaardelijstenLookupService(IOdrcClientFactory clientFactory) : IWaardelijstenLookupService
    {
        public async Task<IReadOnlyDictionary<string, WaardelijstItem>> GetAllAsync(string reden, CancellationToken token)
        {
            var resultaat = new Dictionary<string, WaardelijstItem>();

            using var client = clientFactory.Create(reden);

            await FetchCategorieAsync(client, resultaat, WaardelijstCategorieen.Organisatie, "/api/v2/organisaties", token);
            await FetchCategorieAsync(client, resultaat, WaardelijstCategorieen.Informatiecategorie, "/api/v2/informatiecategorieen", token);
            await FetchCategorieAsync(client, resultaat, WaardelijstCategorieen.Onderwerp, "/api/v2/onderwerpen", token);

            return resultaat;
        }

        private static async Task FetchCategorieAsync(
            HttpClient client,
            Dictionary<string, WaardelijstItem> resultaat,
            string categorie,
            string startUrl,
            CancellationToken token)
        {
            string? url = startUrl;

            while (url != null)
            {
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadFromJsonAsync<PagedResponseModel<JsonObject>>(token);

                if (json?.Results == null) return;

                foreach (var item in json.Results)
                {
                    var uuid = item["uuid"]?.GetValue<string>();
                    var naam = item["naam"]?.GetValue<string>() ?? item["officieleTitel"]?.GetValue<string>();

                    if (uuid != null && naam != null)
                    {
                        resultaat[uuid] = new WaardelijstItem(categorie, naam);
                    }
                }

                url = UrlHelper.GetPathAndQuery(json.Next);
            }
        }
    }
}
