using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pea.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterNumberToMeterReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeterNumber",
                table: "MeterReadings",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeterNumber",
                table: "MeterReadings");
        }
    }
}
