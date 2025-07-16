using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails
{

    [ApiController]
    [Authorize(AdminPolicy.Name)]
    public class GebruikersgroepDetailsController(OdpcDbContext context) : ControllerBase
    {
        private readonly OdpcDbContext _context = context;

        [HttpGet("api/gebruikersgroepen/{uuid:guid}")]
        public async Task<IActionResult> Get(Guid uuid, CancellationToken token)
        {
            var groep = await _context.Gebruikersgroepen
                .Include(x => x.Waardelijsten)
                .Include(x => x.GebruikersgroepGebruikers)
                .SingleOrDefaultAsync(x => x.Uuid == uuid, cancellationToken: token);

            return groep == null ? NotFound() : Ok(GebruikersgroepDetailsModel.MapEntityToViewModel(groep));
        }
    }
}
