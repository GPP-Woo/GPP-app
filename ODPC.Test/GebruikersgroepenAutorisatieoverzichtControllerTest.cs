using Microsoft.AspNetCore.Mvc;
using ODPC.Apis.Odrc;
using ODPC.Data.Entities;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepenAutorisatieoverzicht;

namespace ODPC.Test
{
    [TestClass]
    public class GebruikersgroepenAutorisatieoverzichtControllerTest
    {
        private class StubWaardelijstenLookupService : IWaardelijstenLookupService
        {
            public Task<IReadOnlyDictionary<string, WaardelijstItem>> GetAllAsync(string reden, CancellationToken token) =>
                Task.FromResult<IReadOnlyDictionary<string, WaardelijstItem>>(new Dictionary<string, WaardelijstItem>());
        }

        [TestMethod]
        public async Task Get_retourneert_csv_bestand_met_bestandsnaam()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var lookup = new StubWaardelijstenLookupService();
            var controller = new GebruikersgroepenAutorisatieoverzichtController(context, lookup);

            var result = await controller.Get(default);

            if (result is not FileContentResult fileResult)
            {
                Assert.Fail();
                return;
            }

            Assert.AreEqual("text/csv", fileResult.ContentType);
            Assert.IsTrue(fileResult.FileDownloadName.StartsWith("autorisatieoverzicht-gebruikersgroepen-"));
            Assert.IsTrue(fileResult.FileDownloadName.EndsWith(".csv"));
        }

        [TestMethod]
        public async Task Get_bevat_alle_gebruikersgroepen_in_de_csv()
        {
            using var context = InMemoryDatabase.GetDbContext();

            var groep = new Gebruikersgroep { Uuid = Guid.NewGuid(), Naam = Guid.NewGuid().ToString() };

            await context.AddAsync(groep);
            await context.SaveChangesAsync();

            var lookup = new StubWaardelijstenLookupService();
            var controller = new GebruikersgroepenAutorisatieoverzichtController(context, lookup);

            var result = await controller.Get(default);

            if (result is not FileContentResult fileResult)
            {
                Assert.Fail();
                return;
            }

            var csv = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);

            StringAssert.Contains(csv, groep.Naam);
        }

        [TestMethod]
        public async Task Get_start_csv_met_utf8_bom_zodat_excel_speciale_tekens_herkent()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var lookup = new StubWaardelijstenLookupService();
            var controller = new GebruikersgroepenAutorisatieoverzichtController(context, lookup);

            var result = await controller.Get(default);

            if (result is not FileContentResult fileResult)
            {
                Assert.Fail();
                return;
            }

            var bom = fileResult.FileContents.Take(3).ToArray();

            CollectionAssert.AreEqual(new byte[] { 0xEF, 0xBB, 0xBF }, bom);
        }
    }
}
