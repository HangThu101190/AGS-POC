using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDepartmentAllowedRolesJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Previous migration used defaultValue "" which is not a JSON array.
            migrationBuilder.Sql("""
                UPDATE departments
                SET allowed_roles_json = '[]'::jsonb
                WHERE jsonb_typeof(allowed_roles_json) <> 'array';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "allowed_roles_json",
                table: "departments",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "allowed_roles_json",
                table: "departments",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");
        }
    }
}
