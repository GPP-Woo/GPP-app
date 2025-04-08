using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Onderwerpen.MijnOnderwerpen
{
    public class MijnOnderwerpenController(IOdrcClientFactory clientFactory, IGebruikerWaardelijstItemsService waardelijstItemsService) : ControllerBase
    {
        [HttpGet("api/{version}/mijn-onderwerpen")]
        public async IAsyncEnumerable<JsonObject> Get(string version, [EnumeratorCancellation] CancellationToken token)
        {
            var onderwerpen = await waardelijstItemsService.GetAsync(token);

            if (onderwerpen.Count == 0) yield break;

            using var client = clientFactory.Create("Eigen onderwerpen ophalen");
            var url = $"/api/{version}/onderwerpen?publicatiestatus=concept,gepubliceerd";

            // omdat we zelf moeten filteren obv van de waardelijstitems waar de gebruiker toegang toe heeft,
            // kunnen we geen paginering gebruiker. we lopen door alle pagina's van de ODRC
            while (!string.IsNullOrWhiteSpace(url))
            {
                var page = await client.GetFromJsonAsync<PagedResponseModel<JsonObject>>(url, token) ?? new() { Results = [] };
                foreach (var item in page.Results)
                {
                    if (item["uuid"]?.GetValue<string>() is string uuid && onderwerpen.Contains(uuid))
                    {
                        yield return item;
                    }
                }

                url = UrlHelper.GetPathAndQuery(page?.Next);
            }
        }
    }
}
