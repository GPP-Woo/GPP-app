using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Data;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepenAutorisatieoverzicht
{
    [ApiController]
    [Authorize(AdminPolicy.Name)]
    public class GebruikersgroepenAutorisatieoverzichtController(
        OdpcDbContext context,
        IWaardelijstenLookupService waardelijstenLookup) : ControllerBase
    {
        [HttpGet("api/gebruikersgroepen/autorisatieoverzicht")]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var groepen = await GebruikersgroepDetailsModel
                .MapToViewModel(context.Gebruikersgroepen.OrderBy(x => x.Naam))
                .ToListAsync(token);

            var waardelijsten = await waardelijstenLookup.GetAllAsync("Autorisatieoverzicht gebruikersgroepen genereren", token);

            var bestandsnaam = $"autorisatieoverzicht-gebruikersgroepen-{DateTimeOffset.UtcNow:yyyyMMdd-HHmm}.csv";

            return File(AutorisatieoverzichtCsvBuilder.BuildBytes(groepen, waardelijsten), "text/csv", bestandsnaam);
        }
    }
}
