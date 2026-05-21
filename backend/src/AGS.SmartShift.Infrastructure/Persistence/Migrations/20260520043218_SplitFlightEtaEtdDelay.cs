using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitFlightEtaEtdDelay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "eta_delay_minutes",
                table: "flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "etd_delay_minutes",
                table: "flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE flights
                SET eta_delay_minutes = delay_minutes,
                    etd_delay_minutes = delay_minutes
                WHERE delay_minutes > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "eta_delay_minutes",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "etd_delay_minutes",
                table: "flights");
        }
    }
}
