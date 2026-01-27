using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ODPC.Data.Entities;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepDetails;

namespace ODPC.Test
{
    [TestClass]
    public class GebruikersgroepDetailsTest
    {
        [TestMethod]
        public async Task GebruikersgroepDetails_mapt_id_naam_en_omschrijving()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var groep = Randomgebruikersgroep();
            await context.AddAsync(groep);
            await context.SaveChangesAsync();
            var controller = new GebruikersgroepDetailsController(context);
            var result = await controller.Get(groep.Uuid, default);
            if (result is not OkObjectResult objectResult || objectResult.Value is not GebruikersgroepDetailsModel model)
            {
                Assert.Fail();
                return;
            }
            Assert.AreEqual(groep.Uuid, model.Uuid);
            Assert.AreEqual(groep.Naam, model.Naam);
            Assert.AreEqual(groep.Omschrijving, model.Omschrijving);
        }

        [TestMethod]
        public async Task GebruikersgroepDetails_mapt_waardelijsten()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var groep = Randomgebruikersgroep();
            var waardelijsten = new List<GebruikersgroepWaardelijst>
            {
                RandomWaardeLijstItem(groep),
                RandomWaardeLijstItem(groep),
                RandomWaardeLijstItem(groep),
            };
            await context.AddAsync(groep);
            await context.AddRangeAsync(waardelijsten);
            await context.SaveChangesAsync();
            var controller = new GebruikersgroepDetailsController(context);
            var result = await controller.Get(groep.Uuid, default);
            if (result is not OkObjectResult objectResult || objectResult.Value is not GebruikersgroepDetailsModel model)
            {
                Assert.Fail();
                return;
            }

            Assert.AreEqual(waardelijsten.Count, model.GekoppeldeWaardelijsten.Count());

            foreach (var item in waardelijsten)
            {
                Assert.IsTrue(model.GekoppeldeWaardelijsten.Contains(item.WaardelijstId));
            }
        }

        [TestMethod]
        public async Task GebruikersgroepDetails_retourneert_notfound_voof_onbekende_uuid()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var controller = new GebruikersgroepDetailsController(context);
            var result = await controller.Get(Guid.NewGuid(), default);
            if (result is not IStatusCodeActionResult statusCodeResult)
            {
                Assert.Fail();
                return;
            }

            Assert.AreEqual(404, statusCodeResult.StatusCode);
        }

        private static GebruikersgroepWaardelijst RandomWaardeLijstItem(Gebruikersgroep groep) => new()
        {
            GebruikersgroepUuid = groep.Uuid,
            WaardelijstId = Guid.NewGuid().ToString()
        };

        private static Gebruikersgroep Randomgebruikersgroep() => new()
        {
            Uuid = Guid.NewGuid(),
            Naam = Guid.NewGuid().ToString(),
            Omschrijving = Guid.NewGuid().ToString()
        };
    }
}
