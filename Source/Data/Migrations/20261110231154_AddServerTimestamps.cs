using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodSugarBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServerTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SrvCreated",
                table: "Treatments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SrvModified",
                table: "Treatments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SrvCreated",
                table: "BglEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SrvModified",
                table: "BglEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SrvCreated",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "SrvModified",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "SrvCreated",
                table: "BglEntries");

            migrationBuilder.DropColumn(
                name: "SrvModified",
                table: "BglEntries");
        }
    }
}
