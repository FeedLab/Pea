using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pea.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RateA = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RateB = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RateC = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeterReadings");
        }
    }
}
