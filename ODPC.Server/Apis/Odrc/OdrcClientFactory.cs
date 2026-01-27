using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using ODPC.Authentication;

namespace ODPC.Apis.Odrc
{
    public interface IOdrcClientFactory
    {
        HttpClient Create(string handeling);
    }

    public class OdrcClientFactory(IHttpClientFactory httpClientFactory, IConfiguration config, OdpcUser user) : IOdrcClientFactory
    {
        public HttpClient Create(string? handeling)
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new(config["ODRC_BASE_URL"]!);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", config["ODRC_API_KEY"]);
            client.DefaultRequestHeaders.Add("Audit-User-ID", NormalizeToValidAscii(user.Id));
            client.DefaultRequestHeaders.Add("Audit-User-Representation", NormalizeToValidAscii(user.FullName));
            client.DefaultRequestHeaders.Add("Audit-Remarks", NormalizeToValidAscii(handeling));
            return client;
        }

        private static string? NormalizeToValidAscii(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            if (Ascii.IsValid(input)) return input;

            var normalized = input.Normalize(NormalizationForm.FormD);

            if (normalized == input) return input;

            var builder = new StringBuilder(capacity: normalized.Length);

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
