﻿using System.Net.Http;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Documenten.UploadBestandsdeel
{
    [ApiController]
    [DisableRequestSizeLimit]
    // [RequestSizeLimit(52428800)]
    public class UploadBestandsdeelController(IOdrcClientFactory clientFactory) : ControllerBase
    {
        [HttpPut("api/v1/documenten/{docUuid:guid}/bestandsdelen/{partUuid:guid}")]
        public async Task<IActionResult> Put(Guid docUuid, Guid partUuid, CancellationToken token)
        {
            if (Request.Form.Files.Count == 0)
            {
                return BadRequest("No file uploaded");
            }

            var file = Request.Form.Files[0];
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());

            content.Add(fileContent, "inhoud", file.FileName);

            using var client = clientFactory.Create("Upload bestandsdeel");
            var response = await client.PutAsync("/api/v1/documenten/" + docUuid + "/bestandsdelen/" + partUuid, content, token);

            response.EnsureSuccessStatusCode();

            return NoContent();
        }
    }
}
