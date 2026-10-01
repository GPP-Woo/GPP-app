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

    public class WaardelijstenLookupService(
        IOdrcClientFactory clientFactory,
        ILogger<WaardelijstenLookupService> logger) : IWaardelijstenLookupService
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

        private async Task FetchCategorieAsync(
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

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
                    logger.LogError(
                        "Waardelijst '{Categorie}' ophalen mislukt. Status: {Status}, Body: {Body}",
                        categorie, response.StatusCode, body);
                    response.EnsureSuccessStatusCode();
                }

                var json = await response.Content.ReadFromJsonAsync<PagedResponseModel<JsonObject>>(token);

                if (json?.Results == null)
                {
                    logger.LogError(
                        "Waardelijst '{Categorie}' ophalen gaf een leeg of onverwacht antwoord.",
                        categorie);
                    throw new InvalidOperationException(
                        $"Waardelijst '{categorie}' gaf een leeg of onverwacht antwoord.");
                }

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
