using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockAdjustmentHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReasonCode = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_StockAdjustmentHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentHeaders_VoucherHeaders_LinkedVoucherId",
                        column: x => x.LinkedVoucherId,
                        principalTable: "VoucherHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentHeaders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StockAdjustmentHeaderId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    BaseQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LineValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentLines_StockAdjustmentHeaders_StockAdjustmentHeaderId",
                        column: x => x.StockAdjustmentHeaderId,
                        principalTable: "StockAdjustmentHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentHeaders_LinkedVoucherId",
                table: "StockAdjustmentHeaders",
                column: "LinkedVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentHeaders_TenantId_BranchId_AdjustmentNo",
                table: "StockAdjustmentHeaders",
                columns: new[] { "TenantId", "BranchId", "AdjustmentNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentHeaders_WarehouseId",
                table: "StockAdjustmentHeaders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentLines_ProductVariantId",
                table: "StockAdjustmentLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentLines_StockAdjustmentHeaderId",
                table: "StockAdjustmentLines",
                column: "StockAdjustmentHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentLines_TenantId_BranchId",
                table: "StockAdjustmentLines",
                columns: new[] { "TenantId", "BranchId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockAdjustmentLines");

            migrationBuilder.DropTable(
                name: "StockAdjustmentHeaders");
        }
    }
}
