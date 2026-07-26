using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicesAndOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExternalReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    ReferenceInvoiceId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    PaymentTermDays = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    FulfillmentStatus = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedVoucherId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceHeaders_BusinessPartners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceHeaders_InvoiceHeaders_ReferenceInvoiceId",
                        column: x => x.ReferenceInvoiceId,
                        principalTable: "InvoiceHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceHeaders_VoucherHeaders_LinkedVoucherId",
                        column: x => x.LinkedVoucherId,
                        principalTable: "VoucherHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceHeaders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    InvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BaseQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCostAtSale = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TaxRateId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FulfilledBaseQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReferenceInvoiceLineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_InvoiceHeaders_InvoiceHeaderId",
                        column: x => x.InvoiceHeaderId,
                        principalTable: "InvoiceHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_InvoiceLines_ReferenceInvoiceLineId",
                        column: x => x.ReferenceInvoiceLineId,
                        principalTable: "InvoiceLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_LinkedVoucherId",
                table: "InvoiceHeaders",
                column: "LinkedVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_PartnerId",
                table: "InvoiceHeaders",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_ReferenceInvoiceId",
                table: "InvoiceHeaders",
                column: "ReferenceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_TenantId_BranchId_InvoiceType_InvoiceNo",
                table: "InvoiceHeaders",
                columns: new[] { "TenantId", "BranchId", "InvoiceType", "InvoiceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_TenantId_BranchId_InvoiceType_Status",
                table: "InvoiceHeaders",
                columns: new[] { "TenantId", "BranchId", "InvoiceType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_WarehouseId",
                table: "InvoiceHeaders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceHeaderId",
                table: "InvoiceLines",
                column: "InvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ProductVariantId",
                table: "InvoiceLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ReferenceInvoiceLineId",
                table: "InvoiceLines",
                column: "ReferenceInvoiceLineId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_TaxRateId",
                table: "InvoiceLines",
                column: "TaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_TenantId_BranchId",
                table: "InvoiceLines",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_UnitOfMeasureId",
                table: "InvoiceLines",
                column: "UnitOfMeasureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "InvoiceHeaders");
        }
    }
}
