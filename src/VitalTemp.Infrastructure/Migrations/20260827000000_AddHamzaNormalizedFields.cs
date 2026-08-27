using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalTemp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHamzaNormalizedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "temp_normalized",
                table: "temperature_readings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "normalized_value",
                table: "health_data",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "composite_risk_score",
                table: "analysis_results",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "temp_normalized",
                table: "temperature_readings");

            migrationBuilder.DropColumn(
                name: "normalized_value",
                table: "health_data");

            migrationBuilder.DropColumn(
                name: "composite_risk_score",
                table: "analysis_results");
        }
    }
}
