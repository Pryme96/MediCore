using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrescrizioneOriginAssistita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OriginAssistita",
                table: "Prescrizioni",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginAssistita",
                table: "Prescrizioni");
        }
    }
}
