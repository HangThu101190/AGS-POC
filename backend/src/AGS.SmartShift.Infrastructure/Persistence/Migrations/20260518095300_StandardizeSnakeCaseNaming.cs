using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeSnakeCaseNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_sites_site_id",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "FK_employees_departments_department_id",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_shift_slots_weekly_plans_WeeklyPlanId",
                table: "shift_slots");

            migrationBuilder.DropForeignKey(
                name: "FK_user_accounts_employees_EmployeeId",
                table: "user_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_weekly_plans",
                table: "weekly_plans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_accounts",
                table: "user_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sites",
                table: "sites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shift_slots",
                table: "shift_slots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notification_messages",
                table: "notification_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_flights",
                table: "flights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_departments",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_events",
                table: "audit_events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attendance_records",
                table: "attendance_records");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "weekly_plans",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "weekly_plans",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WeekYear",
                table: "weekly_plans",
                newName: "week_year");

            migrationBuilder.RenameColumn(
                name: "WeekId",
                table: "weekly_plans",
                newName: "week_id");

            migrationBuilder.RenameColumn(
                name: "TodayIdx",
                table: "weekly_plans",
                newName: "today_idx");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "weekly_plans",
                newName: "site_id");

            migrationBuilder.RenameColumn(
                name: "PublishedAtUtc",
                table: "weekly_plans",
                newName: "published_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_weekly_plans_WeekId",
                table: "weekly_plans",
                newName: "ix_weekly_plans_week_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "user_accounts",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "user_accounts",
                newName: "row_version");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "user_accounts",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LoginName",
                table: "user_accounts",
                newName: "login_name");

            migrationBuilder.RenameColumn(
                name: "LockoutEndUtc",
                table: "user_accounts",
                newName: "lockout_end_utc");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "user_accounts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "FailedLoginCount",
                table: "user_accounts",
                newName: "failed_login_count");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "user_accounts",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "user_accounts",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_user_accounts_LoginName",
                table: "user_accounts",
                newName: "ix_user_accounts_login_name");

            migrationBuilder.RenameIndex(
                name: "IX_user_accounts_EmployeeId",
                table: "user_accounts",
                newName: "ix_user_accounts_employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_sites_code",
                table: "sites",
                newName: "ix_sites_code");

            migrationBuilder.RenameColumn(
                name: "Headcount",
                table: "shift_slots",
                newName: "headcount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "shift_slots",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WeeklyPlanId",
                table: "shift_slots",
                newName: "weekly_plan_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentCode",
                table: "shift_slots",
                newName: "department_code");

            migrationBuilder.RenameColumn(
                name: "DayIdx",
                table: "shift_slots",
                newName: "day_idx");

            migrationBuilder.RenameColumn(
                name: "BucketKey",
                table: "shift_slots",
                newName: "bucket_key");

            migrationBuilder.RenameIndex(
                name: "IX_shift_slots_WeeklyPlanId_DayIdx_DepartmentCode",
                table: "shift_slots",
                newName: "ix_shift_slots_weekly_plan_id_day_idx_department_code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserAccountId",
                table: "refresh_tokens",
                newName: "user_account_id");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "refresh_tokens",
                newName: "token_hash");

            migrationBuilder.RenameColumn(
                name: "RevokedAtUtc",
                table: "refresh_tokens",
                newName: "revoked_at_utc");

            migrationBuilder.RenameColumn(
                name: "ExpiresAtUtc",
                table: "refresh_tokens",
                newName: "expires_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_UserAccountId_ExpiresAtUtc",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_account_id_expires_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token_hash");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notification_messages",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "notification_messages",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "notification_messages",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notification_messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "notification_messages",
                newName: "is_read");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "notification_messages",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_notification_messages_EmployeeId_IsRead",
                table: "notification_messages",
                newName: "ix_notification_messages_employee_id_is_read");

            migrationBuilder.RenameColumn(
                name: "Std",
                table: "flights",
                newName: "std");

            migrationBuilder.RenameColumn(
                name: "Sta",
                table: "flights",
                newName: "sta");

            migrationBuilder.RenameColumn(
                name: "Route",
                table: "flights",
                newName: "route");

            migrationBuilder.RenameColumn(
                name: "Remark",
                table: "flights",
                newName: "remark");

            migrationBuilder.RenameColumn(
                name: "Parking",
                table: "flights",
                newName: "parking");

            migrationBuilder.RenameColumn(
                name: "Manning",
                table: "flights",
                newName: "manning");

            migrationBuilder.RenameColumn(
                name: "Gate",
                table: "flights",
                newName: "gate");

            migrationBuilder.RenameColumn(
                name: "Etd",
                table: "flights",
                newName: "etd");

            migrationBuilder.RenameColumn(
                name: "Eta",
                table: "flights",
                newName: "eta");

            migrationBuilder.RenameColumn(
                name: "Belt",
                table: "flights",
                newName: "belt");

            migrationBuilder.RenameColumn(
                name: "Aircraft",
                table: "flights",
                newName: "aircraft");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "flights",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WeekId",
                table: "flights",
                newName: "week_id");

            migrationBuilder.RenameColumn(
                name: "VipNote",
                table: "flights",
                newName: "vip_note");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "flights",
                newName: "site_id");

            migrationBuilder.RenameColumn(
                name: "ManningExplain",
                table: "flights",
                newName: "manning_explain");

            migrationBuilder.RenameColumn(
                name: "IsVip",
                table: "flights",
                newName: "is_vip");

            migrationBuilder.RenameColumn(
                name: "IsDelayed",
                table: "flights",
                newName: "is_delayed");

            migrationBuilder.RenameColumn(
                name: "FlightNo",
                table: "flights",
                newName: "flight_no");

            migrationBuilder.RenameColumn(
                name: "DepartureFlightNo",
                table: "flights",
                newName: "departure_flight_no");

            migrationBuilder.RenameColumn(
                name: "DepartmentCode",
                table: "flights",
                newName: "department_code");

            migrationBuilder.RenameColumn(
                name: "DelayMinutes",
                table: "flights",
                newName: "delay_minutes");

            migrationBuilder.RenameColumn(
                name: "DayIdx",
                table: "flights",
                newName: "day_idx");

            migrationBuilder.RenameIndex(
                name: "IX_flights_WeekId_DayIdx",
                table: "flights",
                newName: "ix_flights_week_id_day_idx");

            migrationBuilder.RenameIndex(
                name: "IX_flights_FlightNo",
                table: "flights",
                newName: "ix_flights_flight_no");

            migrationBuilder.RenameIndex(
                name: "IX_employees_department_id",
                table: "employees",
                newName: "ix_employees_department_id");

            migrationBuilder.RenameIndex(
                name: "IX_employees_code",
                table: "employees",
                newName: "ix_employees_code");

            migrationBuilder.RenameIndex(
                name: "IX_departments_site_id_code",
                table: "departments",
                newName: "ix_departments_site_id_code");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "audit_events",
                newName: "path");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "audit_events",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserAccountId",
                table: "audit_events",
                newName: "user_account_id");

            migrationBuilder.RenameColumn(
                name: "StatusCode",
                table: "audit_events",
                newName: "status_code");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "audit_events",
                newName: "occurred_at_utc");

            migrationBuilder.RenameColumn(
                name: "HttpMethod",
                table: "audit_events",
                newName: "http_method");

            migrationBuilder.RenameColumn(
                name: "EmployeeCode",
                table: "audit_events",
                newName: "employee_code");

            migrationBuilder.RenameIndex(
                name: "IX_audit_events_OccurredAtUtc",
                table: "audit_events",
                newName: "ix_audit_events_occurred_at_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "attendance_records",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "attendance_records",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "attendance_records",
                newName: "row_version");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "attendance_records",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "attendance_records",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CheckInUtc",
                table: "attendance_records",
                newName: "check_in_utc");

            migrationBuilder.RenameIndex(
                name: "IX_attendance_records_EmployeeId",
                table: "attendance_records",
                newName: "ix_attendance_records_employee_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_weekly_plans",
                table: "weekly_plans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_accounts",
                table: "user_accounts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sites",
                table: "sites",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shift_slots",
                table: "shift_slots",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notification_messages",
                table: "notification_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_flights",
                table: "flights",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employees",
                table: "employees",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_departments",
                table: "departments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_events",
                table: "audit_events",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_attendance_records",
                table: "attendance_records",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_departments_sites_site_id",
                table: "departments",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_employees_departments_department_id",
                table: "employees",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shift_slots_weekly_plans_weekly_plan_id",
                table: "shift_slots",
                column: "weekly_plan_id",
                principalTable: "weekly_plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_accounts_employees_employee_id",
                table: "user_accounts",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_departments_sites_site_id",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "fk_employees_departments_department_id",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "fk_shift_slots_weekly_plans_weekly_plan_id",
                table: "shift_slots");

            migrationBuilder.DropForeignKey(
                name: "fk_user_accounts_employees_employee_id",
                table: "user_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_weekly_plans",
                table: "weekly_plans");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_accounts",
                table: "user_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sites",
                table: "sites");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shift_slots",
                table: "shift_slots");

            migrationBuilder.DropPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notification_messages",
                table: "notification_messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_flights",
                table: "flights");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employees",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "pk_departments",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_events",
                table: "audit_events");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attendance_records",
                table: "attendance_records");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "weekly_plans",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "weekly_plans",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "week_year",
                table: "weekly_plans",
                newName: "WeekYear");

            migrationBuilder.RenameColumn(
                name: "week_id",
                table: "weekly_plans",
                newName: "WeekId");

            migrationBuilder.RenameColumn(
                name: "today_idx",
                table: "weekly_plans",
                newName: "TodayIdx");

            migrationBuilder.RenameColumn(
                name: "site_id",
                table: "weekly_plans",
                newName: "SiteId");

            migrationBuilder.RenameColumn(
                name: "published_at_utc",
                table: "weekly_plans",
                newName: "PublishedAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_weekly_plans_week_id",
                table: "weekly_plans",
                newName: "IX_weekly_plans_WeekId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "user_accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "user_accounts",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "row_version",
                table: "user_accounts",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "user_accounts",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "login_name",
                table: "user_accounts",
                newName: "LoginName");

            migrationBuilder.RenameColumn(
                name: "lockout_end_utc",
                table: "user_accounts",
                newName: "LockoutEndUtc");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "user_accounts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "failed_login_count",
                table: "user_accounts",
                newName: "FailedLoginCount");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "user_accounts",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "user_accounts",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_user_accounts_login_name",
                table: "user_accounts",
                newName: "IX_user_accounts_LoginName");

            migrationBuilder.RenameIndex(
                name: "ix_user_accounts_employee_id",
                table: "user_accounts",
                newName: "IX_user_accounts_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "ix_sites_code",
                table: "sites",
                newName: "IX_sites_code");

            migrationBuilder.RenameColumn(
                name: "headcount",
                table: "shift_slots",
                newName: "Headcount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "shift_slots",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "weekly_plan_id",
                table: "shift_slots",
                newName: "WeeklyPlanId");

            migrationBuilder.RenameColumn(
                name: "department_code",
                table: "shift_slots",
                newName: "DepartmentCode");

            migrationBuilder.RenameColumn(
                name: "day_idx",
                table: "shift_slots",
                newName: "DayIdx");

            migrationBuilder.RenameColumn(
                name: "bucket_key",
                table: "shift_slots",
                newName: "BucketKey");

            migrationBuilder.RenameIndex(
                name: "ix_shift_slots_weekly_plan_id_day_idx_department_code",
                table: "shift_slots",
                newName: "IX_shift_slots_WeeklyPlanId_DayIdx_DepartmentCode");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "refresh_tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_account_id",
                table: "refresh_tokens",
                newName: "UserAccountId");

            migrationBuilder.RenameColumn(
                name: "token_hash",
                table: "refresh_tokens",
                newName: "TokenHash");

            migrationBuilder.RenameColumn(
                name: "revoked_at_utc",
                table: "refresh_tokens",
                newName: "RevokedAtUtc");

            migrationBuilder.RenameColumn(
                name: "expires_at_utc",
                table: "refresh_tokens",
                newName: "ExpiresAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_account_id_expires_at_utc",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_UserAccountId_ExpiresAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_TokenHash");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "notification_messages",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "notification_messages",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "notification_messages",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "notification_messages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_read",
                table: "notification_messages",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "notification_messages",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "ix_notification_messages_employee_id_is_read",
                table: "notification_messages",
                newName: "IX_notification_messages_EmployeeId_IsRead");

            migrationBuilder.RenameColumn(
                name: "std",
                table: "flights",
                newName: "Std");

            migrationBuilder.RenameColumn(
                name: "sta",
                table: "flights",
                newName: "Sta");

            migrationBuilder.RenameColumn(
                name: "route",
                table: "flights",
                newName: "Route");

            migrationBuilder.RenameColumn(
                name: "remark",
                table: "flights",
                newName: "Remark");

            migrationBuilder.RenameColumn(
                name: "parking",
                table: "flights",
                newName: "Parking");

            migrationBuilder.RenameColumn(
                name: "manning",
                table: "flights",
                newName: "Manning");

            migrationBuilder.RenameColumn(
                name: "gate",
                table: "flights",
                newName: "Gate");

            migrationBuilder.RenameColumn(
                name: "etd",
                table: "flights",
                newName: "Etd");

            migrationBuilder.RenameColumn(
                name: "eta",
                table: "flights",
                newName: "Eta");

            migrationBuilder.RenameColumn(
                name: "belt",
                table: "flights",
                newName: "Belt");

            migrationBuilder.RenameColumn(
                name: "aircraft",
                table: "flights",
                newName: "Aircraft");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "flights",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "week_id",
                table: "flights",
                newName: "WeekId");

            migrationBuilder.RenameColumn(
                name: "vip_note",
                table: "flights",
                newName: "VipNote");

            migrationBuilder.RenameColumn(
                name: "site_id",
                table: "flights",
                newName: "SiteId");

            migrationBuilder.RenameColumn(
                name: "manning_explain",
                table: "flights",
                newName: "ManningExplain");

            migrationBuilder.RenameColumn(
                name: "is_vip",
                table: "flights",
                newName: "IsVip");

            migrationBuilder.RenameColumn(
                name: "is_delayed",
                table: "flights",
                newName: "IsDelayed");

            migrationBuilder.RenameColumn(
                name: "flight_no",
                table: "flights",
                newName: "FlightNo");

            migrationBuilder.RenameColumn(
                name: "departure_flight_no",
                table: "flights",
                newName: "DepartureFlightNo");

            migrationBuilder.RenameColumn(
                name: "department_code",
                table: "flights",
                newName: "DepartmentCode");

            migrationBuilder.RenameColumn(
                name: "delay_minutes",
                table: "flights",
                newName: "DelayMinutes");

            migrationBuilder.RenameColumn(
                name: "day_idx",
                table: "flights",
                newName: "DayIdx");

            migrationBuilder.RenameIndex(
                name: "ix_flights_week_id_day_idx",
                table: "flights",
                newName: "IX_flights_WeekId_DayIdx");

            migrationBuilder.RenameIndex(
                name: "ix_flights_flight_no",
                table: "flights",
                newName: "IX_flights_FlightNo");

            migrationBuilder.RenameIndex(
                name: "ix_employees_department_id",
                table: "employees",
                newName: "IX_employees_department_id");

            migrationBuilder.RenameIndex(
                name: "ix_employees_code",
                table: "employees",
                newName: "IX_employees_code");

            migrationBuilder.RenameIndex(
                name: "ix_departments_site_id_code",
                table: "departments",
                newName: "IX_departments_site_id_code");

            migrationBuilder.RenameColumn(
                name: "path",
                table: "audit_events",
                newName: "Path");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "audit_events",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_account_id",
                table: "audit_events",
                newName: "UserAccountId");

            migrationBuilder.RenameColumn(
                name: "status_code",
                table: "audit_events",
                newName: "StatusCode");

            migrationBuilder.RenameColumn(
                name: "occurred_at_utc",
                table: "audit_events",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "http_method",
                table: "audit_events",
                newName: "HttpMethod");

            migrationBuilder.RenameColumn(
                name: "employee_code",
                table: "audit_events",
                newName: "EmployeeCode");

            migrationBuilder.RenameIndex(
                name: "ix_audit_events_occurred_at_utc",
                table: "audit_events",
                newName: "IX_audit_events_OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "attendance_records",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "attendance_records",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "row_version",
                table: "attendance_records",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "attendance_records",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "attendance_records",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "check_in_utc",
                table: "attendance_records",
                newName: "CheckInUtc");

            migrationBuilder.RenameIndex(
                name: "ix_attendance_records_employee_id",
                table: "attendance_records",
                newName: "IX_attendance_records_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_weekly_plans",
                table: "weekly_plans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_accounts",
                table: "user_accounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sites",
                table: "sites",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shift_slots",
                table: "shift_slots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification_messages",
                table: "notification_messages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_flights",
                table: "flights",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                table: "employees",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_departments",
                table: "departments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_events",
                table: "audit_events",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_attendance_records",
                table: "attendance_records",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_departments_sites_site_id",
                table: "departments",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_employees_departments_department_id",
                table: "employees",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_shift_slots_weekly_plans_WeeklyPlanId",
                table: "shift_slots",
                column: "WeeklyPlanId",
                principalTable: "weekly_plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_accounts_employees_EmployeeId",
                table: "user_accounts",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
