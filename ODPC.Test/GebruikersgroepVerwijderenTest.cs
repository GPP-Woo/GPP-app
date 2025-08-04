using Microsoft.EntityFrameworkCore;
using ODPC.Data.Entities;
using ODPC.Features.Gebruikersgroepen.GebruikersgroepVerwijderen;

namespace ODPC.Test
{
    [TestClass]
    public class GebruikersgroepVerwijderenTest
    {
        [TestMethod]
        public async Task Test()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var groep = new Gebruikersgroep { Uuid = Guid.NewGuid(), Naam = Guid.NewGuid().ToString(), Omschrijving = Guid.NewGuid().ToString() };
            var waardelijst = new GebruikersgroepWaardelijst { GebruikersgroepUuid = groep.Uuid, WaardelijstId = Guid.NewGuid().ToString() };
            await context.AddRangeAsync(groep, waardelijst);
            await context.SaveChangesAsync();

            var controller = new GebruikersgroepVerwijderenController(context);
            var initialGroepenCount = await context.Gebruikersgroepen.CountAsync();
            var initialWaardelijstenCount = await context.GebruikersgroepWaardelijsten.CountAsync();

            await controller.Delete(groep.Uuid, default);

            Assert.AreEqual(initialGroepenCount - 1, await context.Gebruikersgroepen.CountAsync());
            Assert.AreEqual(initialWaardelijstenCount - 1, await context.GebruikersgroepWaardelijsten.CountAsync());
        }
    }
}
