using Microsoft.EntityFrameworkCore;
using ODPC.Data.Entities;

namespace ODPC.Data
{
    public class OdpcDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Gebruikersgroep> Gebruikersgroepen { get; set; }
        public DbSet<GebruikersgroepWaardelijst> GebruikersgroepWaardelijsten { get; set; }
        public DbSet<GebruikersgroepGebruiker> GebruikersgroepGebruikers { get; set; }
        public DbSet<GebruikersgroepPublicatie> GebruikersgroepPublicatie { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PostgreSQL-specific collation
            if (Database.IsNpgsql())
            {
                modelBuilder.HasCollation("nl_case_insensitive",
                    locale: "nl-NL-u-ks-primary",
                    provider: "icu",
                    deterministic: false);
            }

            modelBuilder.Entity<Gebruikersgroep>().HasKey(t => new { t.Uuid });
            modelBuilder.Entity<Gebruikersgroep>().HasIndex(t => t.Naam).IsUnique();
            modelBuilder.Entity<GebruikersgroepWaardelijst>().HasKey(t => new { t.GebruikersgroepUuid, t.WaardelijstId });
            modelBuilder.Entity<GebruikersgroepGebruiker>().HasKey(t => new { t.GebruikerId, t.GebruikersgroepUuid });
            modelBuilder.Entity<GebruikersgroepPublicatie>().HasKey(t => new { t.GebruikersgroepUuid, t.PublicatieUuid });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            if (Database.IsNpgsql())
            {
                configurationBuilder.Properties<string>().UseCollation("nl_case_insensitive");
            }
        }
    }
}
