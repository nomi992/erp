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

export type RecurringFrequency = 'Daily' | 'Weekly' | 'Monthly' | 'Yearly';

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

export interface VoucherAttachment {
  id: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAtUtc: string;
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
  reversalOfVoucherId: number | null;
  reversalOfVoucherNo: string | null;
  createdBy: string;
  createdAtUtc: string;
  approvedBy: string | null;
  approvedAtUtc: string | null;
  totalDebit: number;
  totalCredit: number;
  lines: VoucherLineResponse[];
  attachments: VoucherAttachment[];
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

export interface RecurringTemplateLineRequest {
  accountId: number;
  debitAmount: number;
  creditAmount: number;
  costCenterId: number | null;
}

export interface RecurringVoucherTemplateRequest {
  name: string;
  voucherType: VoucherType;
  narrationTemplate: string;
  frequency: RecurringFrequency;
  nextRunDate: string;
  lines: RecurringTemplateLineRequest[];
}

export interface RecurringTemplateLineResponse {
  id: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  debitAmount: number;
  creditAmount: number;
  costCenterId: number | null;
  costCenterName: string | null;
}

export interface RecurringVoucherTemplate {
  id: number;
  name: string;
  voucherType: VoucherType;
  narrationTemplate: string;
  frequency: RecurringFrequency;
  nextRunDate: string;
  isActive: boolean;
  lines: RecurringTemplateLineResponse[];
}
