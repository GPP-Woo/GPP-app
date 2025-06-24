using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;

namespace ODPC.Features.Documenten.DocumentVerwijderen
{
    [ApiController]
    public class DocumentVerwijderenController(IOdrcClientFactory clientFactory, OdpcUser user) : ControllerBase
    {
        [HttpDelete("api/{version}/documenten/{uuid:guid}")]
        public async Task<IActionResult> Delete(string version, Guid uuid, CancellationToken token)
        {
            using var client = clientFactory.Create("Document verwijderen");

            var url = $"/api/{version}/documenten/{uuid}";

            // document ophalen
            using var getResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!getResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var json = await getResponse.Content.ReadFromJsonAsync<PublicatieDocument>(token);

            if (json?.Eigenaar?.identifier != user.Id)
            {
                return NotFound();
            }

            // document verwijderen
            using var deleteResponse = await client.DeleteAsync(url, token);

            return !deleteResponse.IsSuccessStatusCode ? StatusCode(502) : StatusCode(204);
        }
    }
}
