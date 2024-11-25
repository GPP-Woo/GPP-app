﻿using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Informatiecategorieen.AlleInformatiecategorieen
{
    [ApiController]
    public class InformatiecategorieenController(IOdrcClientFactory clientFactory) : ControllerBase
    {
        [HttpGet("api/{version}/informatiecategorieen")]
        public async Task<IActionResult> Get(string version, CancellationToken token, [FromQuery] string? page = "1")
        {
            // infocategorien ophalen uit het ODRC
            using var client = clientFactory.Create("Informatiecategorieen ophalen");
            var url = $"/api/{version}/informatiecategorieen?page={page}";

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead, token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await response.Content.ReadFromJsonAsync<PagedResponseModel<JsonNode>>(token);

            return Ok(json);
        }
    }
}
