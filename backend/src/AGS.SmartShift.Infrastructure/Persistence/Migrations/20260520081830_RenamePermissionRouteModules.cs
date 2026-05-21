using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGS.SmartShift.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePermissionRouteModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Align permission.module with FE route keys (SystemPermission.Create stores lowercase).
            migrationBuilder.Sql("""
                UPDATE system_permissions
                SET module = 'bangphanca',
                    name_vi = 'Bảng phân ca · phát hành',
                    name_en = 'Shift board · publish'
                WHERE code = 'plan.publish'
                  AND module IN ('planner', 'bangphanca');

                UPDATE system_permissions
                SET module = 'phancongslot',
                    name_vi = 'Phân công slot',
                    name_en = 'Slot assignments'
                WHERE code = 'assignments.mutate'
                  AND module IN ('supboard', 'phancongslot');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE system_permissions
                SET module = 'planner',
                    name_vi = 'Phát hành kế hoạch',
                    name_en = 'Publish weekly plan'
                WHERE code = 'plan.publish'
                  AND module = 'bangphanca';

                UPDATE system_permissions
                SET module = 'supboard',
                    name_vi = 'Phân NV / gán FLT',
                    name_en = 'Assign staff and flights'
                WHERE code = 'assignments.mutate'
                  AND module = 'phancongslot';
                """);
        }
    }
}
