using Microsoft.AspNetCore.Mvc;

namespace ODPC.Features.Formats.FormatsOverzicht
{
    [ApiController]
    public class FormatsOverzichtController : ControllerBase
    {
        [HttpGet("/api/formats")]
        public IActionResult Get()
        {
            return Ok(FormatsMock.Formats.Values.OrderBy(x => x.Name));
        }
    }
}
