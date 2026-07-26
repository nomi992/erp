using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyAndBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VoucherHeaders_VoucherNo",
                table: "VoucherHeaders");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_AccountId_Year_Month",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Code",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "VoucherHeaders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "VoucherHeaders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "VoucherDetails",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "VoucherDetails",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "VoucherAttachments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "VoucherAttachments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "TaxRates",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "ReportSchedules",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ReportSchedules",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RecurringVoucherTemplates",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "RecurringVoucherTemplates",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RecurringVoucherTemplateLines",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "RecurringVoucherTemplateLines",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "FiscalPeriods",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CostCenters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "BankStatementLines",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "BankStatementLines",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name", "Code", "IsActive", "CreatedAtUtc" },
                values: new object[] { 1, "Default", "DEFAULT", true, new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "TenantId", "Name", "Code", "IsActive", "CreatedAtUtc" },
                values: new object[] { 1, 1, "Default Branch", "MAIN", true, new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateTable(
                name: "UserBranches",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBranches", x => new { x.UserId, x.BranchId });
                    table.ForeignKey(
                        name: "FK_UserBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBranches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherHeaders_TenantId_BranchId",
                table: "VoucherHeaders",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherHeaders_TenantId_BranchId_VoucherNo",
                table: "VoucherHeaders",
                columns: new[] { "TenantId", "BranchId", "VoucherNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDetails_TenantId_BranchId",
                table: "VoucherDetails",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherAttachments_TenantId_BranchId",
                table: "VoucherAttachments",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TenantId",
                table: "TaxRates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_TenantId_BranchId",
                table: "ReportSchedules",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringVoucherTemplates_TenantId_BranchId",
                table: "RecurringVoucherTemplates",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringVoucherTemplateLines_TenantId_BranchId",
                table: "RecurringVoucherTemplateLines",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_TenantId",
                table: "FiscalPeriods",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_TenantId",
                table: "CostCenters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_AccountId",
                table: "Budgets",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TenantId_BranchId_AccountId_Year_Month",
                table: "Budgets",
                columns: new[] { "TenantId", "BranchId", "AccountId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_TenantId_BranchId",
                table: "BankStatementLines",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId",
                table: "AuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_Code",
                table: "Accounts",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_Code",
                table: "Branches",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                table: "Tenants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBranches_BranchId",
                table: "UserBranches",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserBranches");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_VoucherHeaders_TenantId_BranchId",
                table: "VoucherHeaders");

            migrationBuilder.DropIndex(
                name: "IX_VoucherHeaders_TenantId_BranchId_VoucherNo",
                table: "VoucherHeaders");

            migrationBuilder.DropIndex(
                name: "IX_VoucherDetails_TenantId_BranchId",
                table: "VoucherDetails");

            migrationBuilder.DropIndex(
                name: "IX_VoucherAttachments_TenantId_BranchId",
                table: "VoucherAttachments");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_TenantId",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_ReportSchedules_TenantId_BranchId",
                table: "ReportSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RecurringVoucherTemplates_TenantId_BranchId",
                table: "RecurringVoucherTemplates");

            migrationBuilder.DropIndex(
                name: "IX_RecurringVoucherTemplateLines_TenantId_BranchId",
                table: "RecurringVoucherTemplateLines");

            migrationBuilder.DropIndex(
                name: "IX_FiscalPeriods_TenantId",
                table: "FiscalPeriods");

            migrationBuilder.DropIndex(
                name: "IX_CostCenters_TenantId",
                table: "CostCenters");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_AccountId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TenantId_BranchId_AccountId_Year_Month",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_BankStatementLines_TenantId_BranchId",
                table: "BankStatementLines");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_TenantId_Code",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "VoucherHeaders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VoucherHeaders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "VoucherDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VoucherDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "VoucherAttachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VoucherAttachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "ReportSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ReportSchedules");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RecurringVoucherTemplates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RecurringVoucherTemplates");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RecurringVoucherTemplateLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RecurringVoucherTemplateLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FiscalPeriods");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "BankStatementLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankStatementLines");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherHeaders_VoucherNo",
                table: "VoucherHeaders",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_AccountId_Year_Month",
                table: "Budgets",
                columns: new[] { "AccountId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Code",
                table: "Accounts",
                column: "Code",
                unique: true);
        }
    }
}
