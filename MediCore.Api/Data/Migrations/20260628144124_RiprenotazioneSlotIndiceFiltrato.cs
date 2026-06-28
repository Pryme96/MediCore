using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RiprenotazioneSlotIndiceFiltrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prenotazioni_SlotId",
                table: "Prenotazioni");

            migrationBuilder.CreateIndex(
                name: "IX_Prenotazioni_SlotId",
                table: "Prenotazioni",
                column: "SlotId",
                unique: true,
                filter: "\"Stato\" <> 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prenotazioni_SlotId",
                table: "Prenotazioni");

            migrationBuilder.CreateIndex(
                name: "IX_Prenotazioni_SlotId",
                table: "Prenotazioni",
                column: "SlotId",
                unique: true);
        }
    }
}
