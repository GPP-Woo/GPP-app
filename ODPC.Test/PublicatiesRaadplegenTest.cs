using System.Net;
using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Authentication;
using ODPC.Features.Publicaties;
using ODPC.Features.Publicaties.PublicatieDetails;
using ODPC.Features.Publicaties.PublicatiesOverzicht;

namespace ODPC.Test
{
    [TestClass]
    public class PublicatiesRaadplegenTest
    {
        private const string UserId = "User-Id-1";

        private static readonly Guid s_uuid = Guid.NewGuid();
        private static readonly Guid s_groep = Guid.NewGuid();

        private static readonly OdpcUser s_user = new()
        {
            IsLoggedIn = true,
            IsAdmin = false,
            Id = UserId,
            FullName = "Test",
            Roles = []
        };

        private static Publicatie Create(string? eigenaar, Guid? groep) => new()
        {
            Uuid = s_uuid,
            OfficieleTitel = "Titel",
            Publicatiestatus = "concept",
            Eigenaar = eigenaar == null ? null : new Eigenaar { identifier = eigenaar },
            EigenaarGroep = groep == null ? null : new EigenaarGroep { identifier = groep.ToString(), weergaveNaam = "Groep" }
        };

        private static PagedResponseModel<object> Page() => new() { Results = [new { titel = "x" }], Count = 1 };

        private static Task<IActionResult> GetOverzichtAsync(StubPublicatiebankHandler bank, bool isGebruiker, string? eigenaarGroep = null) =>
            new PublicatiesOverzichtController(bank.ClientFactory, s_user, new StubGebruikersgroepService([], isGebruiker))
                .Get("v2", default, eigenaarGroep: eigenaarGroep);

        private static Task<IActionResult> GetDetailsAsync(StubPublicatiebankHandler bank, bool isGebruiker) =>
            new PublicatieDetailsController(bank.ClientFactory, s_user, new StubGebruikersgroepService([], isGebruiker))
                .Put("v2", s_uuid, default);

        private static int Count(IActionResult result) =>
            ((PagedResponseModel<System.Text.Json.Nodes.JsonNode>)((OkObjectResult)result).Value!).Count;

        [TestMethod]
        public async Task Overzicht_filtert_op_eigenaar_zonder_eigenaargroep()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Get, "/api/v2/publicaties", HttpStatusCode.OK, Page());

            var result = await GetOverzichtAsync(bank, isGebruiker: false);

            Assert.AreEqual(1, Count(result));
            var query = bank.Requests.Single().PathAndQuery;
            StringAssert.Contains(query, $"eigenaar={UserId}");
            Assert.IsFalse(query.Contains("eigenaarGroep"));
        }

        [TestMethod]
        public async Task Overzicht_filtert_op_eigenaargroep_voor_groepsgebruiker()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Get, "/api/v2/publicaties", HttpStatusCode.OK, Page());

            await GetOverzichtAsync(bank, isGebruiker: true, eigenaarGroep: s_groep.ToString());

            var query = bank.Requests.Single().PathAndQuery;
            StringAssert.Contains(query, $"eigenaarGroep={s_groep}");
            Assert.IsFalse(query.Contains("eigenaar="));
        }

        [TestMethod]
        public async Task Overzicht_is_leeg_voor_niet_groepsgebruiker_zonder_publicatiebank_aan_te_roepen()
        {
            var bank = new StubPublicatiebankHandler();

            var result = await GetOverzichtAsync(bank, isGebruiker: false, eigenaarGroep: s_groep.ToString());

            Assert.AreEqual(0, Count(result));
            Assert.AreEqual(0, bank.Requests.Count);
        }

        [TestMethod]
        public async Task Overzicht_is_leeg_bij_ongeldige_eigenaargroep()
        {
            var bank = new StubPublicatiebankHandler();

            var result = await GetOverzichtAsync(bank, isGebruiker: true, eigenaarGroep: "geen-guid");

            Assert.AreEqual(0, Count(result));
            Assert.AreEqual(0, bank.Requests.Count);
        }

        [TestMethod]
        public async Task Overzicht_geeft_502_als_publicatiebank_faalt()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Get, "/api/v2/publicaties", HttpStatusCode.InternalServerError);

            var result = await GetOverzichtAsync(bank, isGebruiker: true);

            Assert.AreEqual(502, (result as StatusCodeResult)?.StatusCode);
        }

        [TestMethod]
        public async Task Details_toont_publicatie_aan_groepsgebruiker()
        {
            var bank = new StubPublicatiebankHandler()
                .Respond(HttpMethod.Get, $"/api/v2/publicaties/{s_uuid}", HttpStatusCode.OK, Create("andere", s_groep));

            Assert.IsInstanceOfType<OkObjectResult>(await GetDetailsAsync(bank, isGebruiker: true));
        }

        [TestMethod]
        public async Task Details_toont_publicatie_aan_eigenaar_ongeacht_hoofdletters()
        {
            var bank = new StubPublicatiebankHandler()
                .Respond(HttpMethod.Get, $"/api/v2/publicaties/{s_uuid}", HttpStatusCode.OK, Create(UserId.ToUpperInvariant(), s_groep));

            Assert.IsInstanceOfType<OkObjectResult>(await GetDetailsAsync(bank, isGebruiker: false));
        }

        [TestMethod]
        public async Task Details_geeft_404_als_gebruiker_geen_eigenaar_en_geen_groepsgebruiker_is()
        {
            var bank = new StubPublicatiebankHandler()
                .Respond(HttpMethod.Get, $"/api/v2/publicaties/{s_uuid}", HttpStatusCode.OK, Create("andere", s_groep));

            Assert.IsInstanceOfType<NotFoundResult>(await GetDetailsAsync(bank, isGebruiker: false));
        }

        [TestMethod]
        public async Task Details_geeft_502_als_publicatiebank_faalt()
        {
            var bank = new StubPublicatiebankHandler()
                .Respond(HttpMethod.Get, $"/api/v2/publicaties/{s_uuid}", HttpStatusCode.InternalServerError);

            Assert.AreEqual(502, (await GetDetailsAsync(bank, isGebruiker: true) as StatusCodeResult)?.StatusCode);
        }
    }
}
