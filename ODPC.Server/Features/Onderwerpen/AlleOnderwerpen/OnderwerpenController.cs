using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Onderwerpen.AlleOnderwerpen
{
    [ApiController]
    public class OnderwerpenController(IOdrcClientFactory clientFactory) : ControllerBase
    {
        [HttpGet("api/{version}/onderwerpen")]
        //public async Task<IActionResult> Get(string version, CancellationToken token, [FromQuery] string? page = "1")
        //{
        //    // onderwerpen ophalen uit het ODRC
        //    using var client = clientFactory.Create("Onderwerpen ophalen");
        //    var url = $"/api/{version}/onderwerpen?page={page}";

        //    using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return StatusCode(502);
        //    }

        //    var json = await response.Content.ReadFromJsonAsync<PagedResponseModel<JsonNode>>(token);

        //    return Ok(json);
        //}
        public IActionResult Get()
        {
            var items = new List<WaardelijstResponseModel>
            {
                new() { Uuid = "4f83c8db-1d28-4c9b-b19f-9a4b5fdde72a", Naam = "Onderwerp 1" },
                new() { Uuid = "0c1a8c4f-5e58-467a-9149-5e527fdc3a83", Naam = "Onderwerp 2" },
                new() { Uuid = "22bc4c1c-87e3-43b7-8726-6893e206f0df", Naam = "Onderwerp 3" },
                new() { Uuid = "c3b27b89-6091-4986-8127-bcd22a960cb1", Naam = "Onderwerp 4" },
                new() { Uuid = "f10e835e-4e13-41d3-90f0-bd37bc0e1b62", Naam = "Onderwerp 5" },
            };

            var response = new PagedResponseModel<WaardelijstResponseModel>
            {
                Results = items,
                Count = items.Count,
                Next = null,
                Previous = null
            };

            return Ok(response);
        }
    }
}
