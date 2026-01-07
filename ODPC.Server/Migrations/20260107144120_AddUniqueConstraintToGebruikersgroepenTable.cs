using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODPC.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToGebruikersgroepenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:nl_case_insensitive", "nl-NL-u-ks-primary,nl-NL-u-ks-primary,icu,False");

            migrationBuilder.AlterColumn<string>(
                name: "WaardelijstId",
                table: "GebruikersgroepWaardelijsten",
                type: "text",
                nullable: false,
                collation: "nl_case_insensitive",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "GebruikerId",
                table: "GebruikersgroepGebruikers",
                type: "text",
                nullable: false,
                collation: "nl_case_insensitive",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Omschrijving",
                table: "Gebruikersgroepen",
                type: "text",
                nullable: true,
                collation: "nl_case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Naam",
                table: "Gebruikersgroepen",
                type: "text",
                nullable: false,
                collation: "nl_case_insensitive",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Gebruikersgroepen_Naam",
                table: "Gebruikersgroepen",
                column: "Naam",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Gebruikersgroepen_Naam",
                table: "Gebruikersgroepen");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:nl_case_insensitive", "nl-NL-u-ks-primary,nl-NL-u-ks-primary,icu,False");

            migrationBuilder.AlterColumn<string>(
                name: "WaardelijstId",
                table: "GebruikersgroepWaardelijsten",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "nl_case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "GebruikerId",
                table: "GebruikersgroepGebruikers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "nl_case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "Omschrijving",
                table: "Gebruikersgroepen",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "nl_case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "Naam",
                table: "Gebruikersgroepen",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "nl_case_insensitive");
        }
    }
}
