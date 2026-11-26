using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Features.Documenten;
using ODPC.Features.Documenten.DocumentenOverzicht;
using ODPC.Features.Publicaties;

namespace ODPC.Test
{
    [TestClass]
    public class DocumentenOverzichtTest
    {
        private const string UserId = "User-Id-1";

        private static readonly Guid s_publicatie = Guid.NewGuid();
        private static readonly Guid s_groep = Guid.NewGuid();
        private static readonly string s_publicatieUrl = $"/api/v2/publicaties/{s_publicatie}";

        private static readonly OdpcUser s_user = new()
        {
            IsLoggedIn = true,
            IsAdmin = false,
            Id = UserId,
            FullName = "Test",
            Roles = []
        };

        private static Publicatie Create(string? eigenaar) => new()
        {
            Uuid = s_publicatie,
            OfficieleTitel = "Titel",
            Publicatiestatus = "concept",
            Eigenaar = eigenaar == null ? null : new Features.Publicaties.Eigenaar { identifier = eigenaar },
            EigenaarGroep = new EigenaarGroep { identifier = s_groep.ToString(), weergaveNaam = "Groep" }
        };

        private static async Task<(IActionResult Result, StubPublicatiebankHandler Bank)> GetAsync(
            string? eigenaar, bool isGebruiker)
        {
            var bank = new StubPublicatiebankHandler()
                .Respond(HttpMethod.Get, s_publicatieUrl, HttpStatusCode.OK, Create(eigenaar))
                .Respond(HttpMethod.Get, "/api/v2/documenten", HttpStatusCode.OK,
                    new PagedResponseModel<object> { Results = [new { officieleTitel = "doc", bestandsnaam = "a.pdf", bestandsformaat = "application/pdf", bestandsomvang = 1 }], Count = 1 });

            var controller = new DocumentenOverzichtController(bank.ClientFactory, new StubGebruikersgroepService([], isGebruiker));

            return (await controller.Get("v2", s_publicatie, s_user, default), bank);
        }

        private static int Count(IActionResult result) =>
            ((PagedResponseModel<PublicatieDocument>)((OkObjectResult)result).Value!).Count;

        [TestMethod]
        public async Task Toont_documenten_aan_groepsgebruiker()
        {
            var (result, bank) = await GetAsync("andere", isGebruiker: true);

            Assert.AreEqual(1, Count(result));
            Assert.IsTrue(bank.Requests.Any(r => r.PathAndQuery.StartsWith("/api/v2/documenten")));
        }

        [TestMethod]
        public async Task Toont_documenten_aan_eigenaar_ongeacht_hoofdletters()
        {
            var (result, _) = await GetAsync(UserId.ToUpperInvariant(), isGebruiker: false);

            Assert.AreEqual(1, Count(result));
        }

        [TestMethod]
        public async Task Geeft_lege_lijst_zonder_documenten_op_te_halen_als_gebruiker_geen_toegang_heeft()
        {
            var (result, bank) = await GetAsync("andere", isGebruiker: false);

            var page = (PagedResponseModel<JsonNode>)((OkObjectResult)result).Value!;
            Assert.AreEqual(0, page.Count);
            Assert.IsFalse(bank.Requests.Any(r => r.PathAndQuery.StartsWith("/api/v2/documenten")));
        }

        [TestMethod]
        public async Task Geeft_502_als_publicatie_niet_opgehaald_kan_worden()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Get, s_publicatieUrl, HttpStatusCode.InternalServerError);
            var controller = new DocumentenOverzichtController(bank.ClientFactory, new StubGebruikersgroepService([], true));

            var result = await controller.Get("v2", s_publicatie, s_user, default);

            Assert.AreEqual(502, (result as StatusCodeResult)?.StatusCode);
        }
    }
}
