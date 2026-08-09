// Subset of erp/src/app/core/vouchers/voucher.models.ts — the mobile app only creates and
// lists Payment/Receipt vouchers (see README.md for the scoping decision), but VoucherType
// itself is the backend's full enum since the list endpoint is filtered by it.
export type VoucherType =
  | 'Payment'
  | 'Receipt'
  | 'Journal'
  | 'Sales'
  | 'Purchase'
  | 'Contra'
  | 'DebitNote'
  | 'CreditNote';

export type VoucherStatus = 'Draft' | 'PendingApproval' | 'Posted' | 'Rejected';

export interface VoucherLineRequest {
  accountId: number;
  debitAmount: number;
  creditAmount: number;
  costCenterId: number | null;
  taxRateId: number | null;
}

export interface CreateVoucherRequest {
  voucherType: VoucherType;
  date: string;
  narration: string;
  currencyCode: string;
  exchangeRate: number;
  lines: VoucherLineRequest[];
}

export interface VoucherLineResponse {
  id: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  debitAmount: number;
  creditAmount: number;
  costCenterId: number | null;
  costCenterName: string | null;
  taxRateId: number | null;
  taxRateName: string | null;
  taxAmount: number;
}

export interface Voucher {
  id: number;
  voucherType: VoucherType;
  voucherNo: string;
  date: string;
  narration: string;
  status: VoucherStatus;
  currencyCode: string;
  exchangeRate: number;
  createdBy: string;
  createdAtUtc: string;
  totalDebit: number;
  totalCredit: number;
  lines: VoucherLineResponse[];
}

export interface VoucherListItem {
  id: number;
  voucherType: VoucherType;
  voucherNo: string;
  date: string;
  narration: string;
  status: VoucherStatus;
  totalDebit: number;
}
