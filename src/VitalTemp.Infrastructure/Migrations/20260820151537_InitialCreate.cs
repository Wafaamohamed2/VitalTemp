using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalTemp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    city = table.Column<string>(type: "TEXT", nullable: false),
                    state = table.Column<string>(type: "TEXT", nullable: false),
                    latitude = table.Column<double>(type: "REAL", nullable: false),
                    longitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    location_id = table.Column<int>(type: "INTEGER", nullable: false),
                    temp_avg_f = table.Column<double>(type: "REAL", nullable: false),
                    health_indicator = table.Column<string>(type: "TEXT", nullable: false),
                    correlation = table.Column<double>(type: "REAL", nullable: false),
                    p_value = table.Column<double>(type: "REAL", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_results_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_data",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    location_id = table.Column<int>(type: "INTEGER", nullable: false),
                    source = table.Column<string>(type: "TEXT", nullable: false),
                    indicator = table.Column<string>(type: "TEXT", nullable: false),
                    value = table.Column<double>(type: "REAL", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_health_data_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "temperature_readings",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    location_id = table.Column<int>(type: "INTEGER", nullable: false),
                    date = table.Column<string>(type: "TEXT", nullable: false),
                    time = table.Column<string>(type: "TEXT", nullable: false),
                    temp_f = table.Column<double>(type: "REAL", nullable: false),
                    temp_c = table.Column<double>(type: "REAL", nullable: false),
                    granularity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temperature_readings", x => x.id);
                    table.ForeignKey(
                        name: "FK_temperature_readings_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analysis_results_location_id",
                table: "analysis_results",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_data_location_id",
                table: "health_data",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_temperature_readings_location_id",
                table: "temperature_readings",
                column: "location_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_results");

            migrationBuilder.DropTable(
                name: "health_data");

            migrationBuilder.DropTable(
                name: "temperature_readings");

            migrationBuilder.DropTable(
                name: "locations");
        }
    }
}
