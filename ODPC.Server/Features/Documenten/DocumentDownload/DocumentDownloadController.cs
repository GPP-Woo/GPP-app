using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Features.Publicaties;

namespace ODPC.Features.Documenten.DocumentDownload
{
    public class DocumentDownloadController(
        IOdrcClientFactory clientFactory,
        OdpcUser user,
        IGebruikersgroepService gebruikersgroepService) : ControllerBase
    {
        [HttpGet("api/{version}/documenten/{uuid:guid}/download")]
        public async Task<IActionResult> Get(string version, Guid uuid, CancellationToken token)
        {
            // document ophalen uit het ODRC

            using var documentClient = clientFactory.Create("Document ophalen");

            using var documentResponse =
                await documentClient.GetAsync($"/api/{version}/documenten/{uuid}", HttpCompletionOption.ResponseHeadersRead, token);

            if (!documentResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var documentJson = await documentResponse.Content.ReadFromJsonAsync<PublicatieDocument>(token);

            if (documentJson == null)
            {
                return NotFound();
            }

            // overkoepelende publicatie ophalen uit het ODRC

            using var publicatieClient = clientFactory.Create("Publicatie ophalen");

            using var publicatieResponse =
                await publicatieClient.GetAsync($"/api/{version}/publicaties/{documentJson.Publicatie}", HttpCompletionOption.ResponseHeadersRead, token);

            if (!publicatieResponse.IsSuccessStatusCode)
            {
                return StatusCode(502);
            }

            var publicatieJson = await publicatieResponse.Content.ReadFromJsonAsync<Publicatie>(token);

            if (publicatieJson == null)
            {
                return NotFound();
            }

            publicatieJson.EigenaarGroep ??=
                await gebruikersgroepService.TryAndGetEigenaarGroepFromOdpcAsync(documentJson.Publicatie, token);

            // gebruiker mag document downloaden als:
            // a. in groep van overkoepelende publicatie zit
            // b. en/of eigenaar van document is

            var isGebruikersgroepGebruiker = Guid.TryParse(publicatieJson.EigenaarGroep?.identifier, out var identifier)
                && await gebruikersgroepService.IsGebruikersgroepGebruikerAsync(identifier, token);

            return isGebruikersgroepGebruiker || documentJson.Eigenaar?.identifier?.ToLowerInvariant() == user.Id?.ToLowerInvariant()
                ? new DocumentDownloadResult(Request.Path, "Document downloaden")
                : NotFound();
        }
    }
}
