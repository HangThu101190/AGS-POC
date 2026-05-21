using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FlightScheduleDayAndFlightSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_flights_week_id_day_idx",
                table: "flights",
                newName: "ix_flights_week_day");

            migrationBuilder.AlterColumn<string>(
                name: "remark",
                table: "flights",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "parking",
                table: "flights",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "gate",
                table: "flights",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "belt",
                table: "flights",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "excel_row_no",
                table: "flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "flight_schedule_days",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_id = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    day_idx = table.Column<int>(type: "integer", nullable: false),
                    source_day_label = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    sheet_remark = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flight_schedule_days", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_flights_week_day_flight_no",
                table: "flights",
                columns: new[] { "week_id", "day_idx", "flight_no" });

            migrationBuilder.CreateIndex(
                name: "ix_flights_week_day_sort",
                table: "flights",
                columns: new[] { "week_id", "day_idx", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_flight_schedule_days_week_id",
                table: "flight_schedule_days",
                column: "week_id");

            migrationBuilder.CreateIndex(
                name: "ux_flight_schedule_days_week_day",
                table: "flight_schedule_days",
                columns: new[] { "week_id", "day_idx" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_schedule_days");

            migrationBuilder.DropIndex(
                name: "ix_flights_week_day_flight_no",
                table: "flights");

            migrationBuilder.DropIndex(
                name: "ix_flights_week_day_sort",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "excel_row_no",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "flights");

            migrationBuilder.RenameIndex(
                name: "ix_flights_week_day",
                table: "flights",
                newName: "ix_flights_week_id_day_idx");

            migrationBuilder.AlterColumn<string>(
                name: "remark",
                table: "flights",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "parking",
                table: "flights",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "gate",
                table: "flights",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "belt",
                table: "flights",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
