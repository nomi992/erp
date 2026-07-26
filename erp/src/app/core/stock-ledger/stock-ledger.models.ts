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

export interface PartnerAging {
  partnerId: number;
  partnerName: string;
  current: number;
  days1To30: number;
  days31To60: number;
  days61To90: number;
  daysOver90: number;
  total: number;
}
