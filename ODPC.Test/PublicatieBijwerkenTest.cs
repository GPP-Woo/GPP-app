using System.Net;
using Microsoft.AspNetCore.Mvc;
using ODPC.Features.Publicaties;
using ODPC.Features.Publicaties.PublicatieBijwerken;

namespace ODPC.Test
{
    [TestClass]
    public class PublicatieBijwerkenTest
    {
        private static readonly Guid s_uuid = Guid.NewGuid();
        private static readonly string s_url = $"/api/v2/publicaties/{s_uuid}";
        private static readonly string s_groep = Guid.NewGuid().ToString();

        private static Publicatie Create(string status) => new()
        {
            Uuid = s_uuid,
            OfficieleTitel = "Titel",
            Publicatiestatus = status,
            EigenaarGroep = new EigenaarGroep { identifier = s_groep, weergaveNaam = "Groep" }
        };

        private static async Task<(IActionResult Result, FakePublicatiebank Bank)> PutAsync(
            Action<FakePublicatiebank> setup, bool isGebruiker = true)
        {
            using var context = InMemoryDatabase.GetDbContext();
            var bank = new FakePublicatiebank();
            setup(bank);

            var controller = new PublicatieBijwerkenController(context, bank.ClientFactory, new StubGebruikersgroepService([], isGebruiker));
            var result = await controller.Put("v2", s_uuid, Create("gepubliceerd"), default);

            return (result, bank);
        }

        [TestMethod]
        public async Task Publiceert_publicatie_via_publicatiebank()
        {
            var (result, bank) = await PutAsync(b => b
                .Respond(HttpMethod.Get, s_url, HttpStatusCode.OK, Create("concept"))
                .Respond(HttpMethod.Put, s_url, HttpStatusCode.OK, Create("gepubliceerd")));

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual("gepubliceerd", ((Publicatie)ok.Value!).Publicatiestatus);

            var put = bank.Requests.Single(r => r.Method == HttpMethod.Put);
            StringAssert.Contains(put.Body, "gepubliceerd");
        }

        [TestMethod]
        public async Task Geeft_502_als_publicatie_niet_opgehaald_kan_worden()
        {
            var (result, bank) = await PutAsync(b => b.Respond(HttpMethod.Get, s_url, HttpStatusCode.InternalServerError));

            Assert.AreEqual(502, (result as StatusCodeResult)?.StatusCode);
            Assert.IsFalse(bank.Requests.Any(r => r.Method == HttpMethod.Put));
        }

        [TestMethod]
        public async Task Wijzigt_niets_als_gebruiker_niet_in_eigenaargroep_zit()
        {
            var (result, bank) = await PutAsync(
                b => b.Respond(HttpMethod.Get, s_url, HttpStatusCode.OK, Create("concept")),
                isGebruiker: false);

            Assert.IsInstanceOfType<NotFoundResult>(result);
            Assert.IsFalse(bank.Requests.Any(r => r.Method == HttpMethod.Put));
        }

        [TestMethod]
        public async Task Geeft_fout_door_als_publiceren_door_publicatiebank_faalt()
        {
            var ex = await Assert.ThrowsExactlyAsync<HttpRequestException>(() => PutAsync(b => b
                .Respond(HttpMethod.Get, s_url, HttpStatusCode.OK, Create("concept"))
                .Respond(HttpMethod.Put, s_url, HttpStatusCode.BadGateway)));

            Assert.AreEqual(HttpStatusCode.BadGateway, ex.StatusCode);
        }
    }
}
