using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfermaPresenzaPaziente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConfermataDalPaziente",
                table: "Prenotazioni",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfermataDalPaziente",
                table: "Prenotazioni");
        }
    }
}
