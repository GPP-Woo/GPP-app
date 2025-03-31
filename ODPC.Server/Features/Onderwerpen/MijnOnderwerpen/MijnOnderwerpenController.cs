using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Onderwerpen.MijnOnderwerpen
{
    public class MijnOnderwerpenController(IOdrcClientFactory clientFactory, IGebruikerWaardelijstItemsService waardelijstItemsService) : ControllerBase
    {
        [HttpGet("api/{version}/mijn-onderwerpen")]
        //public async IAsyncEnumerable<JsonObject> Get(string version, [EnumeratorCancellation] CancellationToken token)
        //{
        //    var onderwerpen = await waardelijstItemsService.GetAsync(token);

        //    if (onderwerpen.Count == 0) yield break;

        //    using var client = clientFactory.Create("Eigen onderwerpen ophalen");
        //    var url = $"/api/{version}/onderwerpen";

        //    // omdat we zelf moeten filteren obv van de waardelijstitems waar de gebruiker toegang toe heeft,
        //    // kunnen we geen paginering gebruiker. we lopen door alle pagina's van de ODRC
        //    while (!string.IsNullOrWhiteSpace(url))
        //    {
        //        var page = await client.GetFromJsonAsync<PagedResponseModel<JsonObject>>(url, token) ?? new() { Results = [] };
        //        foreach (var item in page.Results)
        //        {
        //            if (item["uuid"]?.GetValue<string>() is string uuid && onderwerpen.Contains(uuid))
        //            {
        //                yield return item;
        //            }
        //        }

        //        url = UrlHelper.GetPathAndQuery(page?.Next);
        //    }
        //}
        public async IAsyncEnumerable<WaardelijstResponseModel> Get([EnumeratorCancellation] CancellationToken token)
        {
            var onderwerpen = await waardelijstItemsService.GetAsync(token);

            if (onderwerpen.Count == 0) yield break;

            var items = new List<WaardelijstResponseModel>
            {
                new() { Uuid = "4f83c8db-1d28-4c9b-b19f-9a4b5fdde72a", Naam = "Onderwerp 1" },
                new() { Uuid = "0c1a8c4f-5e58-467a-9149-5e527fdc3a83", Naam = "Onderwerp 2" },
                new() { Uuid = "22bc4c1c-87e3-43b7-8726-6893e206f0df", Naam = "Onderwerp 3" },
                new() { Uuid = "c3b27b89-6091-4986-8127-bcd22a960cb1", Naam = "Onderwerp 4" },
                new() { Uuid = "f10e835e-4e13-41d3-90f0-bd37bc0e1b62", Naam = "Onderwerp 5" },
            };

            foreach (var item in items)
            {
                if (onderwerpen.Contains(item.Uuid))
                {
                    yield return item;
                }
            }
        }
    }
}
