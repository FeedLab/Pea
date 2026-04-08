using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pea.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMeterReadingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_PeriodStart",
                table: "MeterReadings");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_MeterNumber_PeriodStart",
                table: "MeterReadings",
                columns: new[] { "MeterNumber", "PeriodStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_MeterNumber_PeriodStart",
                table: "MeterReadings");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_PeriodStart",
                table: "MeterReadings",
                column: "PeriodStart",
                unique: true);
        }
    }
}
