using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalTemp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueAnalysisResultIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove any pre-existing duplicate analysis rows before enforcing uniqueness.
            // Keeps the row with the smallest id for each (location_id, health_indicator) pair.
            migrationBuilder.Sql(
                "DELETE FROM analysis_results WHERE id NOT IN (SELECT MIN(id) FROM analysis_results GROUP BY location_id, health_indicator);");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_results_location_id_health_indicator",
                table: "analysis_results",
                columns: new[] { "location_id", "health_indicator" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_analysis_results_location_id_health_indicator",
                table: "analysis_results");
        }
    }
}
