using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ODPC.Migrations
{
    /// <inheritdoc />
    public partial class AddGebruikersgroepPublicatie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GebruikersgroepPublicatie",
                columns: table => new
                {
                    GebruikersgroepUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicatieUuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GebruikersgroepPublicatie", x => new { x.GebruikersgroepUuid, x.PublicatieUuid });
                    table.ForeignKey(
                        name: "FK_GebruikersgroepPublicatie_Gebruikersgroepen_Gebruikersgroep~",
                        column: x => x.GebruikersgroepUuid,
                        principalTable: "Gebruikersgroepen",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GebruikersgroepPublicatie");
        }
    }
}
