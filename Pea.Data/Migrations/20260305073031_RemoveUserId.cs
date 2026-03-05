using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pea.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_PeriodStart",
                table: "MeterReadings");

            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_UserId",
                table: "MeterReadings");

            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_UserId_PeriodStart",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MeterReadings");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_PeriodStart",
                table: "MeterReadings",
                column: "PeriodStart",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_PeriodStart",
                table: "MeterReadings");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MeterReadings",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_PeriodStart",
                table: "MeterReadings",
                column: "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_UserId",
                table: "MeterReadings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_UserId_PeriodStart",
                table: "MeterReadings",
                columns: new[] { "UserId", "PeriodStart" },
                unique: true);
        }
    }
}
