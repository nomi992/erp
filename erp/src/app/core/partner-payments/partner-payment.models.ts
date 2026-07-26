export type PaymentDirection = 'CustomerReceipt' | 'SupplierPayment';

export type PartnerPaymentStatus = 'Draft' | 'PendingApproval' | 'Posted' | 'Rejected';

export interface PartnerPaymentAllocationRequest {
  invoiceHeaderId: number;
  allocatedAmount: number;
}

export interface PartnerPaymentRequest {
  direction: PaymentDirection;
  partnerId: number;
  date: string;
  bankOrCashAccountId: number;
  totalAmount: number;
  narration: string | null;
  allocations: PartnerPaymentAllocationRequest[];
}

export interface PartnerPaymentAllocation {
  id: number;
  invoiceHeaderId: number;
  invoiceNo: string;
  allocatedAmount: number;
  invoiceOutstandingAmount: number;
}

export interface PartnerPayment {
  id: number;
  direction: PaymentDirection;
  paymentNo: string;
  partnerId: number;
  partnerName: string;
  date: string;
  bankOrCashAccountId: number;
  bankOrCashAccountName: string;
  totalAmount: number;
  narration: string | null;
  status: PartnerPaymentStatus;
  linkedVoucherId: number | null;
  linkedVoucherNo: string | null;
  createdBy: string;
  createdAtUtc: string;
  approvedBy: string | null;
  approvedAtUtc: string | null;
  allocations: PartnerPaymentAllocation[];
}

export interface PartnerPaymentListItem {
  id: number;
  direction: PaymentDirection;
  paymentNo: string;
  partnerId: number;
  partnerName: string;
  date: string;
  totalAmount: number;
  status: PartnerPaymentStatus;
}
