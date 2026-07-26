using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRightsAndRolesAndUserRoleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Phase 1 (was "AddRightsAndRoles"): pure additive. Creates the Rights/Roles/RoleRights
            // tables and the 3 built-in Role rows, with a fixed CreatedAtUtc literal (not
            // DateTime.UtcNow) so a future `migrations add` never sees this seed as changed.
            // Does not touch Users yet — the backfill below depends on these rows already existing.
            migrationBuilder.CreateTable(
                name: "Rights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleRights",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RightId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRights", x => new { x.RoleId, x.RightId });
                    table.ForeignKey(
                        name: "FK_RoleRights_Rights_RightId",
                        column: x => x.RightId,
                        principalTable: "Rights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleRights_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "IsSystemRole", "Name", "TenantId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Standard application user.", true, "User", null },
                    { 2, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Tenant administrator.", true, "Admin", null },
                    { 3, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Cross-tenant platform administrator.", true, "SystemAdmin", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rights_Code",
                table: "Rights",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleRights_RightId",
                table: "RoleRights",
                column: "RightId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            // --- Phase 2 (was "AddUserRoleId"): Users.Role (string) -> Users.RoleId (FK), hand-edited
            // per the standing warning that EF's autogenerated `defaultValue` for a new non-nullable
            // column is untrustworthy for backfilling existing rows.

            // 1. Add RoleId as nullable, no default.
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: true);

            // 2. Backfill from the string Role column. Safe because Phase 1 above guarantees these
            // built-in Role rows already exist.
            migrationBuilder.Sql(
                "UPDATE u SET u.RoleId = r.Id FROM Users u INNER JOIN Roles r ON r.TenantId IS NULL AND r.Name = u.Role;");

            // 3. Fail loudly rather than silently corrupt data if any user didn't map to a role
            // (e.g. a Role value that isn't one of the 3 built-ins).
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM Users WHERE RoleId IS NULL) THROW 51000, 'One or more Users rows could not be mapped from Role to RoleId during migration.', 1;");

            // 4. Now safe to require it.
            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 5. Drop the old string column now that RoleId is populated and constrained.
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Symmetric reverse of Phase 2: re-add Role, backfill from Roles.Name via Users.RoleId,
            // then drop the RoleId FK/column.
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE u SET u.Role = r.Name FROM Users u INNER JOIN Roles r ON r.Id = u.RoleId;");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "RoleRights");

            migrationBuilder.DropTable(
                name: "Rights");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
