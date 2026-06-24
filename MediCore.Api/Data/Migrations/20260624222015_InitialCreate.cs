using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servizi",
                columns: table => new
                {
                    ServizioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descrizione = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servizi", x => x.ServizioId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pazienti",
                columns: table => new
                {
                    PazienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CodiceFiscale = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    DataNascita = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pazienti", x => x.PazienteId);
                    table.ForeignKey(
                        name: "FK_Pazienti_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medici",
                columns: table => new
                {
                    MedicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Specializzazione = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ServizioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medici", x => x.MedicoId);
                    table.ForeignKey(
                        name: "FK_Medici_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medici_Servizi_ServizioId",
                        column: x => x.ServizioId,
                        principalTable: "Servizi",
                        principalColumn: "ServizioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prestazioni",
                columns: table => new
                {
                    PrestazioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServizioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descrizione = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DurataMinuti = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestazioni", x => x.PrestazioneId);
                    table.ForeignKey(
                        name: "FK_Prestazioni_Servizi_ServizioId",
                        column: x => x.ServizioId,
                        principalTable: "Servizi",
                        principalColumn: "ServizioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PazienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ruolo = table.Column<int>(type: "INTEGER", nullable: false),
                    Contenuto = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Pazienti_PazienteId",
                        column: x => x.PazienteId,
                        principalTable: "Pazienti",
                        principalColumn: "PazienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescrizioni",
                columns: table => new
                {
                    PrescrizioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PazienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataEmissione = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataScadenza = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Farmaci = table.Column<string>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NotificaInviata = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescrizioni", x => x.PrescrizioneId);
                    table.ForeignKey(
                        name: "FK_Prescrizioni_Medici_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medici",
                        principalColumn: "MedicoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescrizioni_Pazienti_PazienteId",
                        column: x => x.PazienteId,
                        principalTable: "Pazienti",
                        principalColumn: "PazienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tariffe",
                columns: table => new
                {
                    TariffaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrestazioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Regime = table.Column<int>(type: "INTEGER", nullable: false),
                    Prezzo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariffe", x => x.TariffaId);
                    table.ForeignKey(
                        name: "FK_Tariffe_Prestazioni_PrestazioneId",
                        column: x => x.PrestazioneId,
                        principalTable: "Prestazioni",
                        principalColumn: "PrestazioneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turni",
                columns: table => new
                {
                    TurnoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrestazioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GiornoSettimana = table.Column<int>(type: "INTEGER", nullable: false),
                    OraInizio = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    OraFine = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    DurataSlotMin = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turni", x => x.TurnoId);
                    table.ForeignKey(
                        name: "FK_Turni_Medici_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medici",
                        principalColumn: "MedicoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turni_Prestazioni_PrestazioneId",
                        column: x => x.PrestazioneId,
                        principalTable: "Prestazioni",
                        principalColumn: "PrestazioneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Slot",
                columns: table => new
                {
                    SlotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TurnoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataOraInizio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataOraFine = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Stato = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slot", x => x.SlotId);
                    table.ForeignKey(
                        name: "FK_Slot_Turni_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turni",
                        principalColumn: "TurnoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prenotazioni",
                columns: table => new
                {
                    PrenotazioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PazienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Regime = table.Column<int>(type: "INTEGER", nullable: false),
                    Stato = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prenotazioni", x => x.PrenotazioneId);
                    table.ForeignKey(
                        name: "FK_Prenotazioni_Pazienti_PazienteId",
                        column: x => x.PazienteId,
                        principalTable: "Pazienti",
                        principalColumn: "PazienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prenotazioni_Slot_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slot",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fatture",
                columns: table => new
                {
                    FatturaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrenotazioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PazienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Importo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Regime = table.Column<int>(type: "INTEGER", nullable: false),
                    DataEmissione = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Stato = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fatture", x => x.FatturaId);
                    table.ForeignKey(
                        name: "FK_Fatture_Pazienti_PazienteId",
                        column: x => x.PazienteId,
                        principalTable: "Pazienti",
                        principalColumn: "PazienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fatture_Prenotazioni_PrenotazioneId",
                        column: x => x.PrenotazioneId,
                        principalTable: "Prenotazioni",
                        principalColumn: "PrenotazioneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referti",
                columns: table => new
                {
                    RefertoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrenotazioneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataEmissione = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Contenuto = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referti", x => x.RefertoId);
                    table.ForeignKey(
                        name: "FK_Referti_Prenotazioni_PrenotazioneId",
                        column: x => x.PrenotazioneId,
                        principalTable: "Prenotazioni",
                        principalColumn: "PrenotazioneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_PazienteId",
                table: "ChatMessages",
                column: "PazienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatture_PazienteId",
                table: "Fatture",
                column: "PazienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatture_PrenotazioneId",
                table: "Fatture",
                column: "PrenotazioneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medici_ServizioId",
                table: "Medici",
                column: "ServizioId");

            migrationBuilder.CreateIndex(
                name: "IX_Medici_UserId",
                table: "Medici",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pazienti_CodiceFiscale",
                table: "Pazienti",
                column: "CodiceFiscale",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pazienti_UserId",
                table: "Pazienti",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prenotazioni_PazienteId",
                table: "Prenotazioni",
                column: "PazienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prenotazioni_SlotId",
                table: "Prenotazioni",
                column: "SlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescrizioni_MedicoId",
                table: "Prescrizioni",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescrizioni_PazienteId",
                table: "Prescrizioni",
                column: "PazienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestazioni_ServizioId",
                table: "Prestazioni",
                column: "ServizioId");

            migrationBuilder.CreateIndex(
                name: "IX_Referti_PrenotazioneId",
                table: "Referti",
                column: "PrenotazioneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slot_TurnoId_DataOraInizio",
                table: "Slot",
                columns: new[] { "TurnoId", "DataOraInizio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tariffe_PrestazioneId_Regime",
                table: "Tariffe",
                columns: new[] { "PrestazioneId", "Regime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turni_MedicoId",
                table: "Turni",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Turni_PrestazioneId",
                table: "Turni",
                column: "PrestazioneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Fatture");

            migrationBuilder.DropTable(
                name: "Prescrizioni");

            migrationBuilder.DropTable(
                name: "Referti");

            migrationBuilder.DropTable(
                name: "Tariffe");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Prenotazioni");

            migrationBuilder.DropTable(
                name: "Pazienti");

            migrationBuilder.DropTable(
                name: "Slot");

            migrationBuilder.DropTable(
                name: "Turni");

            migrationBuilder.DropTable(
                name: "Medici");

            migrationBuilder.DropTable(
                name: "Prestazioni");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Servizi");
        }
    }
}
