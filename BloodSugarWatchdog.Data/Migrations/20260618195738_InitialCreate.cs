// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodSugarWatchdog.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BglDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BglDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BglDirections",
                columns: table => new
                {
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BglDirections", x => x.Type);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BglEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Sgv = table.Column<int>(type: "INTEGER", nullable: false),
                    Delta = table.Column<decimal>(type: "TEXT", nullable: false),
                    DirectionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Filtered = table.Column<int>(type: "INTEGER", nullable: false),
                    Unfiltered = table.Column<int>(type: "INTEGER", nullable: false),
                    Rssi = table.Column<int>(type: "INTEGER", nullable: false),
                    Noise = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    SysTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UtcOffset = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BglEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BglEntries_BglDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "BglDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BglEntries_BglDirections_DirectionType",
                        column: x => x.DirectionType,
                        principalTable: "BglDirections",
                        principalColumn: "Type",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Treatments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    UUID = table.Column<string>(type: "TEXT", nullable: true),
                    Insulin = table.Column<double>(type: "REAL", nullable: true),
                    InsulinType = table.Column<string>(type: "TEXT", nullable: true),
                    InsulinInjections = table.Column<string>(type: "TEXT", nullable: true),
                    Carbs = table.Column<double>(type: "REAL", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: true),
                    SysTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UtcOffset = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Treatments_TreatmentDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "TreatmentDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BglEntries_DeviceId",
                table: "BglEntries",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_BglEntries_DirectionType",
                table: "BglEntries",
                column: "DirectionType");

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_DeviceId",
                table: "Treatments",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BglEntries");

            migrationBuilder.DropTable(
                name: "Treatments");

            migrationBuilder.DropTable(
                name: "BglDevices");

            migrationBuilder.DropTable(
                name: "BglDirections");

            migrationBuilder.DropTable(
                name: "TreatmentDevices");
        }
    }
}
