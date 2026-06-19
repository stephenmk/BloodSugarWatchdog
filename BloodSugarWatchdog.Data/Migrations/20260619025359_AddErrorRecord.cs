// Copyright (c) 2026 Stephen Kraus
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodSugarWatchdog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UniqueIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    RecordJson = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorRecords_UniqueIdentifier",
                table: "ErrorRecords",
                column: "UniqueIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorRecords");
        }
    }
}
