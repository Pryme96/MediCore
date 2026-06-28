using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrescrizioneTipoEPianoTerapeutico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Farmaci",
                table: "Prescrizioni");

            migrationBuilder.AddColumn<string>(
                name: "Diagnosi",
                table: "Prescrizioni",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurataGiorni",
                table: "Prescrizioni",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Monitoraggio",
                table: "Prescrizioni",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Prescrizioni",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RighePrescrizione",
                columns: table => new
                {
                    RigaPrescrizioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrescrizioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Farmaco = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Posologia = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantita = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RighePrescrizione", x => x.RigaPrescrizioneId);
                    table.ForeignKey(
                        name: "FK_RighePrescrizione_Prescrizioni_PrescrizioneId",
                        column: x => x.PrescrizioneId,
                        principalTable: "Prescrizioni",
                        principalColumn: "PrescrizioneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RighePrescrizione_PrescrizioneId",
                table: "RighePrescrizione",
                column: "PrescrizioneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RighePrescrizione");

            migrationBuilder.DropColumn(
                name: "Diagnosi",
                table: "Prescrizioni");

            migrationBuilder.DropColumn(
                name: "DurataGiorni",
                table: "Prescrizioni");

            migrationBuilder.DropColumn(
                name: "Monitoraggio",
                table: "Prescrizioni");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Prescrizioni");

            migrationBuilder.AddColumn<string>(
                name: "Farmaci",
                table: "Prescrizioni",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
