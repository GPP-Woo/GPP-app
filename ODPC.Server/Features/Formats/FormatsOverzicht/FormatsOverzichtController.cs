using Microsoft.AspNetCore.Mvc;

namespace ODPC.Features.Formats.FormatsOverzicht
{
    [ApiController]
    public class FormatsOverzichtController : ControllerBase
    {
        [HttpGet("/api/v1/formats")]
        [HttpGet("/api/v2/formats")]
        public IActionResult Get()
        {
            return Ok(FormatsMock.Formats.Values.OrderBy(x => x.Name));
        }
    }
}
