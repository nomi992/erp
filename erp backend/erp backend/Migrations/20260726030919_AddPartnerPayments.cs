using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerPaymentHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    PaymentNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BankOrCashAccountId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LinkedVoucherId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerPaymentHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerPaymentHeaders_Accounts_BankOrCashAccountId",
                        column: x => x.BankOrCashAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerPaymentHeaders_BusinessPartners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerPaymentHeaders_VoucherHeaders_LinkedVoucherId",
                        column: x => x.LinkedVoucherId,
                        principalTable: "VoucherHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnerPaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PartnerPaymentHeaderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerPaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerPaymentAllocations_InvoiceHeaders_InvoiceHeaderId",
                        column: x => x.InvoiceHeaderId,
                        principalTable: "InvoiceHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerPaymentAllocations_PartnerPaymentHeaders_PartnerPaymentHeaderId",
                        column: x => x.PartnerPaymentHeaderId,
                        principalTable: "PartnerPaymentHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentAllocations_InvoiceHeaderId",
                table: "PartnerPaymentAllocations",
                column: "InvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentAllocations_PartnerPaymentHeaderId",
                table: "PartnerPaymentAllocations",
                column: "PartnerPaymentHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentAllocations_TenantId_BranchId",
                table: "PartnerPaymentAllocations",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentHeaders_BankOrCashAccountId",
                table: "PartnerPaymentHeaders",
                column: "BankOrCashAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentHeaders_LinkedVoucherId",
                table: "PartnerPaymentHeaders",
                column: "LinkedVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentHeaders_PartnerId",
                table: "PartnerPaymentHeaders",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPaymentHeaders_TenantId_BranchId_Direction_PaymentNo",
                table: "PartnerPaymentHeaders",
                columns: new[] { "TenantId", "BranchId", "Direction", "PaymentNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerPaymentAllocations");

            migrationBuilder.DropTable(
                name: "PartnerPaymentHeaders");
        }
    }
}
