using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceGeolocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "check_in_lat",
                table: "attendance_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "check_in_lng",
                table: "attendance_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "check_out_utc",
                table: "attendance_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "current_lat",
                table: "attendance_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "current_lng",
                table: "attendance_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "geo_note",
                table: "attendance_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "geo_simulated",
                table: "attendance_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "in_zone",
                table: "attendance_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "attendance_gps_points",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lat = table.Column<double>(type: "double precision", nullable: false),
                    lng = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance_gps_points", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_gps_points_attendance_record_id_at_utc",
                table: "attendance_gps_points",
                columns: new[] { "attendance_record_id", "at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_gps_points");

            migrationBuilder.DropColumn(
                name: "check_in_lat",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "check_in_lng",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "check_out_utc",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "current_lat",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "current_lng",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "geo_note",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "geo_simulated",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "in_zone",
                table: "attendance_records");
        }
    }
}
