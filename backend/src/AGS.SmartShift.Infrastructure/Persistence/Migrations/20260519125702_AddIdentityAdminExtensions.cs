using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityAdminExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "must_change_password",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "users",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<string>(
                name: "allowed_roles_json",
                table: "departments",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "departments",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "system_permissions",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    module = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name_vi = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name_en = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "system_roles",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name_vi = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name_en = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_roles", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "system_role_permissions",
                columns: table => new
                {
                    role_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    permission_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_role_permissions", x => new { x.role_code, x.permission_code });
                    table.ForeignKey(
                        name: "fk_system_role_permissions_system_permissions_permission_code",
                        column: x => x.permission_code,
                        principalTable: "system_permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_system_role_permissions_system_roles_role_code",
                        column: x => x.role_code,
                        principalTable: "system_roles",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_system_role_permissions_permission_code",
                table: "system_role_permissions",
                column: "permission_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_role_permissions");

            migrationBuilder.DropTable(
                name: "system_permissions");

            migrationBuilder.DropTable(
                name: "system_roles");

            migrationBuilder.DropColumn(
                name: "must_change_password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "users");

            migrationBuilder.DropColumn(
                name: "allowed_roles_json",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "departments");
        }
    }
}
