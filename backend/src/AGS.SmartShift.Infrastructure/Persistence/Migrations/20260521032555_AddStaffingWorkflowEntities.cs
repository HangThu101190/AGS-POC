using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffingWorkflowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "default_segment",
                table: "employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "max_weekly_hours",
                table: "employees",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: false,
                defaultValue: 48m);

            migrationBuilder.AddColumn<string>(
                name: "preferred_shift_code",
                table: "employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lock_reason",
                table: "daily_staffing_plans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "published_at",
                table: "daily_staffing_plans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "published_by",
                table: "daily_staffing_plans",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "daily_staffing_bio_headers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    daily_staffing_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    morning_sup_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    evening_sup_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    radio_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    assigner_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_staffing_bio_headers", x => x.id);
                    table.ForeignKey(
                        name: "fk_daily_staffing_bio_headers_daily_staffing_plans_daily_staff",
                        column: x => x.daily_staffing_plan_id,
                        principalTable: "daily_staffing_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_qualifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    segment = table.Column<int>(type: "integer", nullable: false),
                    crew_role = table.Column<int>(type: "integer", nullable: false),
                    proficiency = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_qualifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shift_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_overnight = table.Column<bool>(type: "boolean", nullable: false),
                    max_hours = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    segment = table.Column<int>(type: "integer", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "staffing_crew_proposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    daily_staffing_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffing_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shift_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    crew_role = table.Column<int>(type: "integer", nullable: false),
                    work_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    work_end = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_overtime = table.Column<bool>(type: "boolean", nullable: false),
                    is_auto_assigned = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staffing_crew_proposals", x => x.id);
                    table.ForeignKey(
                        name: "fk_staffing_crew_proposals_daily_staffing_lines_staffing_line_",
                        column: x => x.staffing_line_id,
                        principalTable: "daily_staffing_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_staffing_crew_proposals_daily_staffing_plans_daily_staffing",
                        column: x => x.daily_staffing_plan_id,
                        principalTable: "daily_staffing_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_daily_staffing_bio_headers_plan_id",
                table: "daily_staffing_bio_headers",
                column: "daily_staffing_plan_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_employee_qualifications_emp_segment_role",
                table: "employee_qualifications",
                columns: new[] { "employee_id", "segment", "crew_role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_shift_templates_site_dept_code",
                table: "shift_templates",
                columns: new[] { "site_id", "department_code", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staffing_crew_proposals_plan_id",
                table: "staffing_crew_proposals",
                column: "daily_staffing_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_staffing_crew_proposals_staffing_line_id",
                table: "staffing_crew_proposals",
                column: "staffing_line_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_staffing_bio_headers");

            migrationBuilder.DropTable(
                name: "employee_qualifications");

            migrationBuilder.DropTable(
                name: "shift_templates");

            migrationBuilder.DropTable(
                name: "staffing_crew_proposals");

            migrationBuilder.DropColumn(
                name: "default_segment",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "max_weekly_hours",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "preferred_shift_code",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "lock_reason",
                table: "daily_staffing_plans");

            migrationBuilder.DropColumn(
                name: "published_at",
                table: "daily_staffing_plans");

            migrationBuilder.DropColumn(
                name: "published_by",
                table: "daily_staffing_plans");
        }
    }
}
