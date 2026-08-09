// 1:1 port of erp/src/app/core/stock-ledger/stock-ledger.models.ts (backend route is
// api/stockledger for both the on-hand snapshot and the movement ledger).
export type MovementType =
  | 'PurchaseReceipt'
  | 'PurchaseReturnIssue'
  | 'SaleIssue'
  | 'SaleReturnReceipt'
  | 'TransferOut'
  | 'TransferIn'
  | 'AdjustmentIncrease'
  | 'AdjustmentDecrease'
  | 'OpeningBalance';

export type SourceDocumentType = 'Invoice' | 'StockTransfer' | 'StockAdjustment';

export interface StockOnHand {
  productVariantId: number;
  productVariantName: string;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantityOnHand: number;
  averageCost: number;
  stockValue: number;
  reorderLevel: number | null;
  isLowStock: boolean;
  lastMovementAtUtc: string | null;
}

export interface StockLedgerEntry {
  id: number;
  productVariantId: number;
  productVariantName: string;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  transactionDate: string;
  movementType: MovementType;
  quantityIn: number;
  quantityOut: number;
  unitCost: number;
  totalCostSigned: number;
  runningQuantity: number;
  runningValue: number;
  sourceDocumentType: SourceDocumentType;
  sourceDocumentId: number;
  narration: string | null;
  createdBy: string;
  createdAtUtc: string;
}
