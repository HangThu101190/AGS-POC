using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyStaffingAndStaffingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "flight_crew_assignment_id",
                table: "attendance_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "late_check_in_note",
                table: "attendance_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "shift_assignment_id",
                table: "attendance_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aircraft_manning_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_pattern = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    base_manning = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aircraft_manning_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "airline_manning_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_prefix = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airline_manning_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_staffing_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_id = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    day_idx = table.Column<int>(type: "integer", nullable: false),
                    department_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    header_json = table.Column<string>(type: "jsonb", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_staffing_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee_day_availabilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_id = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    day_idx = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    allow_overtime = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_day_availabilities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flight_import_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_id = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    day_idx = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    progress_percent = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    result_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flight_import_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leave_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leave_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shift_check_in_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    bucket_key = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    segment_start = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    segment_end = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    check_in_earliest_minutes_before = table.Column<int>(type: "integer", nullable: false),
                    check_in_latest_minutes_after_start = table.Column<int>(type: "integer", nullable: false),
                    check_out_earliest_minutes_before_end = table.Column<int>(type: "integer", nullable: false),
                    check_out_latest_minutes_after_end = table.Column<int>(type: "integer", nullable: false),
                    require_note_when_late = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift_check_in_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_staffing_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    daily_staffing_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    segment = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    target_manning = table.Column<int>(type: "integer", nullable: false),
                    proposed_manning = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_staffing_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_daily_staffing_lines_daily_staffing_plans_daily_staffing_pl",
                        column: x => x.daily_staffing_plan_id,
                        principalTable: "daily_staffing_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_daily_staffing_lines_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leave_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    to_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leave_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_leave_requests_leave_types_leave_type_id",
                        column: x => x.leave_type_id,
                        principalTable: "leave_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_crew_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffing_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    work_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    work_end = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_overtime = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flight_crew_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_flight_crew_assignments_daily_staffing_lines_staffing_line_",
                        column: x => x.staffing_line_id,
                        principalTable: "daily_staffing_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_records_flight_crew_assignment_id",
                table: "attendance_records",
                column: "flight_crew_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_daily_staffing_lines_flight_id",
                table: "daily_staffing_lines",
                column: "flight_id");

            migrationBuilder.CreateIndex(
                name: "uq_daily_staffing_lines_plan_flight_segment",
                table: "daily_staffing_lines",
                columns: new[] { "daily_staffing_plan_id", "flight_id", "segment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_daily_staffing_plans_site_week_day_dept",
                table: "daily_staffing_plans",
                columns: new[] { "site_id", "week_id", "day_idx", "department_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employee_day_availabilities_week_id_day_idx_employee_id",
                table: "employee_day_availabilities",
                columns: new[] { "week_id", "day_idx", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flight_crew_assignments_staffing_line_id_employee_id_role",
                table: "flight_crew_assignments",
                columns: new[] { "staffing_line_id", "employee_id", "role" });

            migrationBuilder.CreateIndex(
                name: "ix_leave_requests_employee_id_from_date_to_date",
                table: "leave_requests",
                columns: new[] { "employee_id", "from_date", "to_date" });

            migrationBuilder.CreateIndex(
                name: "ix_leave_requests_leave_type_id",
                table: "leave_requests",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_leave_types_code",
                table: "leave_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shift_check_in_policies_site_id_department_code_bucket_key",
                table: "shift_check_in_policies",
                columns: new[] { "site_id", "department_code", "bucket_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_manning_rules");

            migrationBuilder.DropTable(
                name: "airline_manning_rules");

            migrationBuilder.DropTable(
                name: "employee_day_availabilities");

            migrationBuilder.DropTable(
                name: "flight_crew_assignments");

            migrationBuilder.DropTable(
                name: "flight_import_jobs");

            migrationBuilder.DropTable(
                name: "leave_requests");

            migrationBuilder.DropTable(
                name: "shift_check_in_policies");

            migrationBuilder.DropTable(
                name: "daily_staffing_lines");

            migrationBuilder.DropTable(
                name: "leave_types");

            migrationBuilder.DropTable(
                name: "daily_staffing_plans");

            migrationBuilder.DropIndex(
                name: "ix_attendance_records_flight_crew_assignment_id",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "flight_crew_assignment_id",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "late_check_in_note",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "shift_assignment_id",
                table: "attendance_records");
        }
    }
}
