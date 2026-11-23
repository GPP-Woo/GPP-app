using System.Net;
using Microsoft.AspNetCore.Mvc;
using ODPC.Features.Publicaties;
using ODPC.Features.Publicaties.PublicatieRegistreren;

namespace ODPC.Test
{
    [TestClass]
    public class PublicatieRegistrerenTest
    {
        private static readonly string s_organisatie = Guid.NewGuid().ToString();
        private static readonly string s_categorie = Guid.NewGuid().ToString();

        private static Publicatie CreatePublicatie() => new()
        {
            OfficieleTitel = "Titel",
            Publisher = s_organisatie,
            InformatieCategorieen = [s_categorie],
            EigenaarGroep = new EigenaarGroep { identifier = Guid.NewGuid().ToString(), weergaveNaam = "Groep" }
        };

        private static PublicatieRegistrerenController CreateController(FakePublicatiebank bank, params string[] toegestaan) =>
            new(bank.ClientFactory, new StubGebruikersgroepService(toegestaan));

        [TestMethod]
        public async Task Registreert_publicatie_in_publicatiebank()
        {
            var created = new Publicatie { Uuid = Guid.NewGuid(), OfficieleTitel = "Titel" };
            var bank = new FakePublicatiebank().Respond(HttpMethod.Post, "/api/v2/publicaties", HttpStatusCode.Created, created);

            var result = await CreateController(bank, s_organisatie, s_categorie).Post("v2", CreatePublicatie(), default);

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(created.Uuid, ((Publicatie)ok.Value!).Uuid);
            Assert.AreEqual(1, bank.Requests.Count);
            StringAssert.Contains(bank.Requests[0].Body, "Titel");
        }

        [TestMethod]
        public async Task Weigert_publicatie_zonder_eigenaargroep_zonder_publicatiebank_aan_te_roepen()
        {
            var bank = new FakePublicatiebank();
            var publicatie = CreatePublicatie();
            publicatie.EigenaarGroep = null;

            var result = await CreateController(bank, s_organisatie, s_categorie).Post("v2", publicatie, default);

            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            Assert.AreEqual(0, bank.Requests.Count);
        }

        [TestMethod]
        public async Task Weigert_niet_geautoriseerde_informatiecategorie_zonder_publicatiebank_aan_te_roepen()
        {
            var bank = new FakePublicatiebank();

            var result = await CreateController(bank, s_organisatie).Post("v2", CreatePublicatie(), default);

            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            Assert.AreEqual(0, bank.Requests.Count);
        }

        [TestMethod]
        public async Task Geeft_fout_door_als_publicatiebank_faalt()
        {
            var bank = new FakePublicatiebank().Respond(HttpMethod.Post, "/api/v2/publicaties", HttpStatusCode.InternalServerError);

            var ex = await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
                CreateController(bank, s_organisatie, s_categorie).Post("v2", CreatePublicatie(), default));

            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.StatusCode);
        }
    }
}
