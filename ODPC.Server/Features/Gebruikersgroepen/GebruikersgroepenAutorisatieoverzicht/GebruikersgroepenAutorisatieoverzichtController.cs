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

            // this call to the Publicatiebank records (via the existing Audit-* headers) who generated
            // the autorisatieoverzicht and when
            var waardelijsten = await waardelijstenLookup.GetAllAsync("Autorisatieoverzicht gebruikersgroepen genereren", token);

            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam"));
            var fileName = $"autorisatieoverzicht-gebruikersgroepen-{localTime:yyyyMMdd-HHmm}.csv";

            return File(AutorisatieoverzichtCsvBuilder.BuildBytes(groepen, waardelijsten), "text/csv", fileName);
        }
    }
}
