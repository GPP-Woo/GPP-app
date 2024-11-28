﻿using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;

namespace ODPC.Features.Documenten.DocumentDownload
{
    public class DocumentDownloadResult(string path, string reason) : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            var token = context.HttpContext.RequestAborted;

            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            using var client = context.HttpContext.RequestServices.GetRequiredService<IOdrcClientFactory>().Create(reason);
            var timeoutInMinutes = int.TryParse(config["ODRC_DOWNLOAD_TIMEOUT_MINUTES"], out var m)
                ? m
                : 10;
            client.Timeout = TimeSpan.FromMinutes(timeoutInMinutes);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, path) { Version = new Version(2, 0) };
            using var httpResponse = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, token); ;

            response.StatusCode = (int)httpResponse.StatusCode;
            response.Headers.ContentLength = httpResponse.Content.Headers.ContentLength;
            response.Headers.ContentDisposition = httpResponse.Content.Headers.ContentDisposition?.ToString();
            response.Headers.ContentType = httpResponse.Content.Headers.ContentType?.ToString();

            await httpResponse.Content.CopyToAsync(response.Body, token);
        }
    }
}
