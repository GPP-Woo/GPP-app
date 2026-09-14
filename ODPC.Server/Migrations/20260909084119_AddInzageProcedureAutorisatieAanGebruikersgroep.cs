using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODPC.Migrations
{
    /// <inheritdoc />
    public partial class AddInzageProcedureAutorisatieAanGebruikersgroep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGeautoriseerdVoorInzageProcedure",
                table: "Gebruikersgroepen",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGeautoriseerdVoorInzageProcedure",
                table: "Gebruikersgroepen");
        }
    }
}
