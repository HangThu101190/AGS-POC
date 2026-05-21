using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightsAndPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DayIdx = table.Column<int>(type: "integer", nullable: false),
                    FlightNo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DepartureFlightNo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Route = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Sta = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Std = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Eta = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Etd = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    DelayMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsDelayed = table.Column<bool>(type: "boolean", nullable: false),
                    Aircraft = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Manning = table.Column<int>(type: "integer", nullable: false),
                    ManningExplain = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsVip = table.Column<bool>(type: "boolean", nullable: false),
                    VipNote = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Gate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Belt = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Parking = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    WeekYear = table.Column<int>(type: "integer", nullable: false),
                    TodayIdx = table.Column<int>(type: "integer", nullable: false),
                    week_dates_json = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shift_slots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeeklyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DayIdx = table.Column<int>(type: "integer", nullable: false),
                    segments_json = table.Column<string>(type: "text", nullable: false),
                    Headcount = table.Column<int>(type: "integer", nullable: false),
                    BucketKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    flight_nos_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shift_slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shift_slots_weekly_plans_WeeklyPlanId",
                        column: x => x.WeeklyPlanId,
                        principalTable: "weekly_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_flights_FlightNo",
                table: "flights",
                column: "FlightNo");

            migrationBuilder.CreateIndex(
                name: "IX_flights_WeekId_DayIdx",
                table: "flights",
                columns: new[] { "WeekId", "DayIdx" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_messages_EmployeeId_IsRead",
                table: "notification_messages",
                columns: new[] { "EmployeeId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_shift_slots_WeeklyPlanId_DayIdx_DepartmentCode",
                table: "shift_slots",
                columns: new[] { "WeeklyPlanId", "DayIdx", "DepartmentCode" });

            migrationBuilder.CreateIndex(
                name: "IX_weekly_plans_WeekId",
                table: "weekly_plans",
                column: "WeekId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flights");

            migrationBuilder.DropTable(
                name: "notification_messages");

            migrationBuilder.DropTable(
                name: "shift_slots");

            migrationBuilder.DropTable(
                name: "weekly_plans");
        }
    }
}
