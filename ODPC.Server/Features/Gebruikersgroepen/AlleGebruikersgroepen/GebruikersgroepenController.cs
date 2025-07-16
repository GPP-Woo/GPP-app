using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODPC.Authentication;
using ODPC.Data;

namespace ODPC.Features.Gebruikersgroepen.AlleGebruikersgroepen
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AdminPolicy.Name)]
    public class GebruikersgroepenController(OdpcDbContext context) : ControllerBase
    {

        private readonly OdpcDbContext _context = context;

        [HttpGet]
        public IAsyncEnumerable<GebruikersgroepModel> Get()
        {
            return _context
              .Gebruikersgroepen
              .OrderBy(x => x.Naam)
              .Select(x => new GebruikersgroepModel { Naam = x.Naam, Uuid = x.Uuid })
              .AsAsyncEnumerable();
        }
    }
}
