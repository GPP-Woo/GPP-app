using Microsoft.EntityFrameworkCore;
using ODPC.Data.Entities;

namespace ODPC.Data
{
    public class OdpcDbContext : DbContext
    {
        public OdpcDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Gebruikersgroep> Gebruikersgroepen { get; set; }
        public DbSet<GebruikersgroepWaardelijst> GebruikersgroepWaardelijsten { get; set; }
        public DbSet<GebruikersgroepGebruiker> GebruikersgroepGebruikers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Gebruikersgroep>().HasKey(t => new { t.Uuid });
            modelBuilder.Entity<GebruikersgroepWaardelijst>().HasKey(t => new { t.GebruikersgroepUuid, t.WaardelijstId });

            modelBuilder.Entity<GebruikersgroepGebruiker>().HasKey(t => new { t.GebruikerId, t.GebruikersgroepUuid });
        }
    }
}
