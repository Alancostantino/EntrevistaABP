using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WB.EntrevistaABP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pasajeros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DNI = table.Column<int>(type: "integer", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pasajeros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pasajeros_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Viajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaLlegada = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Origen = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Destino = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MedioDeTransporte = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CoordinadorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Viajes_Pasajeros_CoordinadorId",
                        column: x => x.CoordinadorId,
                        principalTable: "Pasajeros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasajerosViajes",
                columns: table => new
                {
                    PasajeroId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViajeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasajerosViajes", x => new { x.PasajeroId, x.ViajeId });
                    table.ForeignKey(
                        name: "FK_PasajerosViajes_Pasajeros_PasajeroId",
                        column: x => x.PasajeroId,
                        principalTable: "Pasajeros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasajerosViajes_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pasajeros_DNI",
                table: "Pasajeros",
                column: "DNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pasajeros_UserId",
                table: "Pasajeros",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasajerosViajes_PasajeroId",
                table: "PasajerosViajes",
                column: "PasajeroId");

            migrationBuilder.CreateIndex(
                name: "IX_PasajerosViajes_ViajeId",
                table: "PasajerosViajes",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_CoordinadorId",
                table: "Viajes",
                column: "CoordinadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasajerosViajes");

            migrationBuilder.DropTable(
                name: "Viajes");

            migrationBuilder.DropTable(
                name: "Pasajeros");
        }
    }
}
