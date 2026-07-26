export type StockTransferStatus = 'Draft' | 'PendingApproval' | 'PendingReceipt' | 'Completed' | 'Rejected' | 'Cancelled';

export interface StockTransferLineRequest {
  productVariantId: number;
  unitOfMeasureId: number;
  qty: number;
}

export interface StockTransferRequest {
  sourceWarehouseId: number;
  destinationWarehouseId: number;
  date: string;
  narration: string | null;
  lines: StockTransferLineRequest[];
}

export interface StockTransferReceiveLineRequest {
  lineId: number;
  receivedBaseQty: number;
}

export interface StockTransferReceiveRequest {
  lines: StockTransferReceiveLineRequest[];
}

export interface StockTransferLineResponse {
  id: number;
  productVariantId: number;
  productVariantName: string;
  productName: string;
  unitOfMeasureId: number;
  unitOfMeasureCode: string;
  qty: number;
  baseQty: number;
  unitCostAtTransfer: number;
  receivedBaseQty: number;
}

export interface StockTransfer {
  id: number;
  transferNo: string;
  sourceWarehouseId: number;
  sourceWarehouseName: string;
  destinationWarehouseId: number;
  destinationWarehouseName: string;
  destinationBranchId: number;
  date: string;
  status: StockTransferStatus;
  narration: string | null;
  createdBy: string;
  createdAtUtc: string;
  approvedBy: string | null;
  approvedAtUtc: string | null;
  receivedBy: string | null;
  receivedAtUtc: string | null;
  lines: StockTransferLineResponse[];
}

export interface StockTransferListItem {
  id: number;
  transferNo: string;
  sourceWarehouseName: string;
  destinationWarehouseName: string;
  date: string;
  status: StockTransferStatus;
}
