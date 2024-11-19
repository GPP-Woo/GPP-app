﻿using System;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Publicaties.PublicatieRegistreren
{
    [ApiController]
    public class PublicatieRegistrerenController(IOdrcClientFactory clientFactory, IGebruikerWaardelijstItemsService waardelijstItemsService) : ControllerBase
    {
        [HttpPost("api/{apiVersion}/publicaties")]
        public async Task<IActionResult> Post(string apiVersion, Publicatie publicatie, CancellationToken token)
        {
            var waardelijstItems = await waardelijstItemsService.GetAsync(token);

            if (publicatie.Publisher != null && !waardelijstItems.Contains(publicatie.Publisher))
            {
                ModelState.AddModelError(nameof(publicatie.Publisher), "Gebruiker is niet geautoriseerd voor deze organisatie");
                return BadRequest(ModelState);
            }

            if (publicatie.InformatieCategorieen != null && publicatie.InformatieCategorieen.Any(c => !waardelijstItems.Contains(c)))
            {
                ModelState.AddModelError(nameof(publicatie.InformatieCategorieen), "Gebruiker is niet geautoriseerd voor deze informatiecategorieën");
                return BadRequest(ModelState);
            }

            using var client = clientFactory.Create("Publicatie registreren");
            var url = "/api/" + apiVersion + "/publicaties";

            var response = await client.PostAsJsonAsync(url, publicatie, token);

            response.EnsureSuccessStatusCode();

            var viewModel = await response.Content.ReadFromJsonAsync<Publicatie>(token);

            return Ok(viewModel);
        }
    }
}
