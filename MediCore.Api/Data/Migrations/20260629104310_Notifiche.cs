using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Notifiche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifiche",
                columns: table => new
                {
                    NotificaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DestinatarioUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Titolo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Messaggio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RiferimentoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Letta = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatoInvio = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInvio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Canale = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifiche", x => x.NotificaId);
                    table.ForeignKey(
                        name: "FK_Notifiche_AspNetUsers_DestinatarioUserId",
                        column: x => x.DestinatarioUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifiche_DestinatarioUserId",
                table: "Notifiche",
                column: "DestinatarioUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifiche_RiferimentoId_Tipo",
                table: "Notifiche",
                columns: new[] { "RiferimentoId", "Tipo" },
                unique: true,
                filter: "\"RiferimentoId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifiche");
        }
    }
}
