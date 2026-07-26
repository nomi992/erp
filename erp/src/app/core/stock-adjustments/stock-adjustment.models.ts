export type AdjustmentReasonCode = 'Damage' | 'Expiry' | 'Loss' | 'CountIncrease' | 'CountDecrease' | 'OpeningBalance' | 'Other';

export type AdjustmentDirection = 'Increase' | 'Decrease';

export type StockAdjustmentStatus = 'Draft' | 'PendingApproval' | 'Posted' | 'Rejected';

export interface StockAdjustmentLineRequest {
  productVariantId: number;
  direction: AdjustmentDirection;
  baseQty: number;
  unitCost: number | null;
}

export interface StockAdjustmentRequest {
  warehouseId: number;
  date: string;
  reasonCode: AdjustmentReasonCode;
  narration: string | null;
  lines: StockAdjustmentLineRequest[];
}

export interface StockAdjustmentLineResponse {
  id: number;
  productVariantId: number;
  productVariantName: string;
  productName: string;
  direction: AdjustmentDirection;
  baseQty: number;
  unitCost: number | null;
  lineValue: number;
}

export interface StockAdjustment {
  id: number;
  adjustmentNo: string;
  warehouseId: number;
  warehouseName: string;
  date: string;
  reasonCode: AdjustmentReasonCode;
  status: StockAdjustmentStatus;
  narration: string | null;
  linkedVoucherId: number | null;
  linkedVoucherNo: string | null;
  createdBy: string;
  createdAtUtc: string;
  approvedBy: string | null;
  approvedAtUtc: string | null;
  lines: StockAdjustmentLineResponse[];
}

export interface StockAdjustmentListItem {
  id: number;
  adjustmentNo: string;
  warehouseName: string;
  date: string;
  reasonCode: AdjustmentReasonCode;
  status: StockAdjustmentStatus;
}
