using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeDatabaseNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_accounts_employees_employee_id",
                table: "user_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_accounts",
                table: "user_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notification_messages",
                table: "notification_messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attendance_gps_points",
                table: "attendance_gps_points");

            migrationBuilder.RenameTable(
                name: "user_accounts",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "notification_messages",
                newName: "notifications");

            migrationBuilder.RenameTable(
                name: "attendance_gps_points",
                newName: "attendance_location_samples");

            migrationBuilder.RenameColumn(
                name: "published_at_utc",
                table: "weekly_plans",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "confirmed_at_utc",
                table: "shift_sync_proposals",
                newName: "confirmed_at");

            migrationBuilder.RenameColumn(
                name: "user_account_id",
                table: "refresh_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "revoked_at_utc",
                table: "refresh_tokens",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "expires_at_utc",
                table: "refresh_tokens",
                newName: "expires_at");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_account_id_expires_at_utc",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_id_expires_at");

            migrationBuilder.RenameColumn(
                name: "published_at_utc",
                table: "flight_schedules",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "locked_at_utc",
                table: "flight_schedules",
                newName: "locked_at");

            migrationBuilder.RenameColumn(
                name: "user_account_id",
                table: "audit_events",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "occurred_at_utc",
                table: "audit_events",
                newName: "occurred_at");

            migrationBuilder.RenameIndex(
                name: "ix_audit_events_occurred_at_utc",
                table: "audit_events",
                newName: "ix_audit_events_occurred_at");

            migrationBuilder.RenameColumn(
                name: "check_out_utc",
                table: "attendance_records",
                newName: "check_out_at");

            migrationBuilder.RenameColumn(
                name: "check_in_utc",
                table: "attendance_records",
                newName: "check_in_at");

            migrationBuilder.RenameColumn(
                name: "lockout_end_utc",
                table: "users",
                newName: "lockout_end_at");

            migrationBuilder.RenameIndex(
                name: "ix_user_accounts_login_name",
                table: "users",
                newName: "ix_users_login_name");

            migrationBuilder.RenameIndex(
                name: "ix_user_accounts_employee_id",
                table: "users",
                newName: "ix_users_employee_id");

            migrationBuilder.RenameIndex(
                name: "ix_notification_messages_employee_id_is_read",
                table: "notifications",
                newName: "ix_notifications_employee_id_is_read");

            migrationBuilder.RenameColumn(
                name: "at_utc",
                table: "attendance_location_samples",
                newName: "recorded_at");

            migrationBuilder.RenameIndex(
                name: "ix_attendance_gps_points_attendance_record_id_at_utc",
                table: "attendance_location_samples",
                newName: "ix_attendance_location_samples_attendance_record_id_recorded_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notifications",
                table: "notifications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_attendance_location_samples",
                table: "attendance_location_samples",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_employees_employee_id",
                table: "users",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_employees_employee_id",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attendance_location_samples",
                table: "attendance_location_samples");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "user_accounts");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "notification_messages");

            migrationBuilder.RenameTable(
                name: "attendance_location_samples",
                newName: "attendance_gps_points");

            migrationBuilder.RenameColumn(
                name: "published_at",
                table: "weekly_plans",
                newName: "published_at_utc");

            migrationBuilder.RenameColumn(
                name: "confirmed_at",
                table: "shift_sync_proposals",
                newName: "confirmed_at_utc");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "refresh_tokens",
                newName: "user_account_id");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "refresh_tokens",
                newName: "revoked_at_utc");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "refresh_tokens",
                newName: "expires_at_utc");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_id_expires_at",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_account_id_expires_at_utc");

            migrationBuilder.RenameColumn(
                name: "published_at",
                table: "flight_schedules",
                newName: "published_at_utc");

            migrationBuilder.RenameColumn(
                name: "locked_at",
                table: "flight_schedules",
                newName: "locked_at_utc");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "audit_events",
                newName: "user_account_id");

            migrationBuilder.RenameColumn(
                name: "occurred_at",
                table: "audit_events",
                newName: "occurred_at_utc");

            migrationBuilder.RenameIndex(
                name: "ix_audit_events_occurred_at",
                table: "audit_events",
                newName: "ix_audit_events_occurred_at_utc");

            migrationBuilder.RenameColumn(
                name: "check_out_at",
                table: "attendance_records",
                newName: "check_out_utc");

            migrationBuilder.RenameColumn(
                name: "check_in_at",
                table: "attendance_records",
                newName: "check_in_utc");

            migrationBuilder.RenameColumn(
                name: "lockout_end_at",
                table: "user_accounts",
                newName: "lockout_end_utc");

            migrationBuilder.RenameIndex(
                name: "ix_users_login_name",
                table: "user_accounts",
                newName: "ix_user_accounts_login_name");

            migrationBuilder.RenameIndex(
                name: "ix_users_employee_id",
                table: "user_accounts",
                newName: "ix_user_accounts_employee_id");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_employee_id_is_read",
                table: "notification_messages",
                newName: "ix_notification_messages_employee_id_is_read");

            migrationBuilder.RenameColumn(
                name: "recorded_at",
                table: "attendance_gps_points",
                newName: "at_utc");

            migrationBuilder.RenameIndex(
                name: "ix_attendance_location_samples_attendance_record_id_recorded_at",
                table: "attendance_gps_points",
                newName: "ix_attendance_gps_points_attendance_record_id_at_utc");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_accounts",
                table: "user_accounts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notification_messages",
                table: "notification_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_attendance_gps_points",
                table: "attendance_gps_points",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_accounts_employees_employee_id",
                table: "user_accounts",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
