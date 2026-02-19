using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODPC.Migrations
{
    /// <inheritdoc />
    public partial class AddGebruikersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gebruikers",
                columns: table => new
                {
                    GebruikerId = table.Column<string>(type: "text", nullable: false, collation: "nl_case_insensitive"),
                    Naam = table.Column<string>(type: "text", nullable: true, collation: "nl_case_insensitive"),
                    LastLogin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gebruikers", x => x.GebruikerId);
                });

            // Voeg alle distinct gebruikers uit GebruikersgroepGebruikers toe aan Gebruikers
            migrationBuilder.Sql("""
                INSERT INTO "Gebruikers" ("GebruikerId", "Naam", "LastLogin")
                SELECT DISTINCT "GebruikerId", NULL, NULL::timestamptz
                FROM "GebruikersgroepGebruikers"
                WHERE "GebruikerId" NOT IN (SELECT "GebruikerId" FROM "Gebruikers")
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_GebruikersgroepGebruikers_Gebruikers_GebruikerId",
                table: "GebruikersgroepGebruikers",
                column: "GebruikerId",
                principalTable: "Gebruikers",
                principalColumn: "GebruikerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GebruikersgroepGebruikers_Gebruikers_GebruikerId",
                table: "GebruikersgroepGebruikers");

            migrationBuilder.DropTable(
                name: "Gebruikers");
        }
    }
}
