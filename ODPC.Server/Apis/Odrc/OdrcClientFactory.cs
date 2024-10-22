﻿using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ODPC.Authentication;

namespace ODPC.Apis.Odrc
{
    public interface IOdrcClientFactory
    {
        HttpClient Create(OdpUser user, string handeling);
    }

    public class OdrcClientFactory(IHttpClientFactory httpClientFactory, IOptions<OdrcConfig> options) : IOdrcClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IOptions<OdrcConfig> _options = options;

        public HttpClient Create(OdpUser user, string? handeling)
        {
            var config = _options.Value;
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new(config.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", config.ApiKey); //dit doet nog niets. nog niet geimplementeerd aan odrc kant. nb nog niet duidelijk of dit de juiste manier zal zijn voor het meesturen van het token
            client.DefaultRequestHeaders.Add("Audit-User-ID", user.Id);
            client.DefaultRequestHeaders.Add("Audit-User-Representation", user.FullName);
            client.DefaultRequestHeaders.Add("Audit-Remarks", handeling);
            return client;
        }

    }
}
