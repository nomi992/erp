using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPartnerLedgerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayableAccountId",
                table: "BusinessPartners",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceivableAccountId",
                table: "BusinessPartners",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartners_PayableAccountId",
                table: "BusinessPartners",
                column: "PayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartners_ReceivableAccountId",
                table: "BusinessPartners",
                column: "ReceivableAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_Accounts_PayableAccountId",
                table: "BusinessPartners",
                column: "PayableAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_Accounts_ReceivableAccountId",
                table: "BusinessPartners",
                column: "ReceivableAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_Accounts_PayableAccountId",
                table: "BusinessPartners");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_Accounts_ReceivableAccountId",
                table: "BusinessPartners");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPartners_PayableAccountId",
                table: "BusinessPartners");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPartners_ReceivableAccountId",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "PayableAccountId",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ReceivableAccountId",
                table: "BusinessPartners");
        }
    }
}
