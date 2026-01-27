using ODPC.Data.Entities;
using ODPC.Features.Gebruikersgroepen.AlleGebruikersgroepen;

namespace ODPC.Test
{
    [TestClass]
    public class GebruikersgroepenOverzichtTest
    {
        [TestMethod]
        public async Task GebruikersgroepenOverzicht_mapt_id_en_naam()
        {
            using var context = InMemoryDatabase.GetDbContext();
            var groep = new Gebruikersgroep { Uuid = Guid.NewGuid(), Naam = Guid.NewGuid().ToString(), Omschrijving = Guid.NewGuid().ToString() };
            await context.AddAsync(groep);
            await context.SaveChangesAsync();
            var controller = new GebruikersgroepenController(context);
            var result = controller.Get().ToBlockingEnumerable().Single(x => x.Uuid == groep.Uuid);
            Assert.AreEqual(groep.Uuid, result.Uuid);
            Assert.AreEqual(groep.Naam, result.Naam);
        }
    }
}
