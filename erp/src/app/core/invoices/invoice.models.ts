export type InvoiceType =
  | 'PurchaseOrder'
  | 'PurchaseInvoice'
  | 'PurchaseReturn'
  | 'SalesOrder'
  | 'SalesInvoice'
  | 'SaleReturn';

export type PaymentMode = 'Cash' | 'Credit';

export type InvoicePaymentStatus = 'Unpaid' | 'PartiallyPaid' | 'Paid' | 'Overdue';

export type FulfillmentStatus = 'Open' | 'PartiallyFulfilled' | 'Fulfilled' | 'Cancelled';

export type InvoiceStatus = 'Draft' | 'PendingApproval' | 'Posted' | 'Rejected' | 'Cancelled';

export interface InvoiceLineRequest {
  productVariantId: number;
  unitOfMeasureId: number;
  qty: number;
  unitAmount: number;
  taxRateId: number | null;
  referenceInvoiceLineId: number | null;
}

export interface InvoiceRequest {
  invoiceType: InvoiceType;
  externalReferenceNo: string | null;
  partnerId: number;
  referenceInvoiceId: number | null;
  warehouseId: number;
  date: string;
  paymentMode: PaymentMode;
  paymentTermDays: number;
  requestedDeliveryDate: string | null;
  narration: string | null;
  lines: InvoiceLineRequest[];
}

export interface InvoiceLineResponse {
  id: number;
  productVariantId: number;
  productVariantName: string;
  productName: string;
  unitOfMeasureId: number;
  unitOfMeasureCode: string;
  qty: number;
  baseQty: number;
  unitAmount: number;
  unitCostAtSale: number | null;
  taxRateId: number | null;
  taxRateName: string | null;
  taxAmount: number;
  lineTotal: number;
  fulfilledBaseQty: number;
  referenceInvoiceLineId: number | null;
}

export interface Invoice {
  id: number;
  invoiceType: InvoiceType;
  invoiceNo: string;
  externalReferenceNo: string | null;
  partnerId: number;
  partnerName: string;
  referenceInvoiceId: number | null;
  referenceInvoiceNo: string | null;
  warehouseId: number;
  warehouseName: string;
  date: string;
  paymentMode: PaymentMode;
  paymentTermDays: number;
  dueDate: string;
  amountPaid: number;
  outstandingAmount: number;
  paymentStatus: InvoicePaymentStatus;
  fulfillmentStatus: FulfillmentStatus | null;
  status: InvoiceStatus;
  narration: string | null;
  linkedVoucherId: number | null;
  linkedVoucherNo: string | null;
  createdBy: string;
  createdAtUtc: string;
  approvedBy: string | null;
  approvedAtUtc: string | null;
  totalNet: number;
  totalTax: number;
  totalAmount: number;
  lines: InvoiceLineResponse[];
}

export interface InvoiceListItem {
  id: number;
  invoiceType: InvoiceType;
  invoiceNo: string;
  partnerId: number;
  partnerName: string;
  date: string;
  dueDate: string;
  outstandingAmount: number;
  paymentStatus: InvoicePaymentStatus;
  fulfillmentStatus: FulfillmentStatus | null;
  status: InvoiceStatus;
  totalAmount: number;
}
