// Subset of erp/src/app/core/invoices/invoice.models.ts — the mobile app only creates and
// lists SalesInvoice documents (see README.md scoping), but InvoiceType/InvoiceStatus mirror
// the backend's full enums since list/filter still goes through the shared endpoint.
export type InvoiceType = 'PurchaseOrder' | 'PurchaseInvoice' | 'PurchaseReturn' | 'SalesOrder' | 'SalesInvoice' | 'SaleReturn';

export type PaymentMode = 'Cash' | 'Credit';

export type InvoicePaymentStatus = 'Unpaid' | 'PartiallyPaid' | 'Paid' | 'Overdue';

export type InvoiceStatus = 'Draft' | 'PendingApproval' | 'Posted' | 'Rejected' | 'Cancelled';

export interface InvoiceLineRequest {
  productVariantId: number;
  unitOfMeasureId: number;
  qty: number;
  unitAmount: number;
  taxRateId: number | null;
  referenceInvoiceLineId: null;
}

export interface InvoiceRequest {
  invoiceType: InvoiceType;
  externalReferenceNo: string | null;
  partnerId: number;
  referenceInvoiceId: null;
  warehouseId: number;
  date: string;
  paymentMode: PaymentMode;
  paymentTermDays: number;
  requestedDeliveryDate: null;
  narration: string | null;
  lines: InvoiceLineRequest[];
}

export interface InvoiceLineResponse {
  id: number;
  productVariantId: number;
  productVariantName: string;
  productName: string;
  unitOfMeasureCode: string;
  qty: number;
  unitAmount: number;
  taxRateId: number | null;
  taxRateName: string | null;
  taxAmount: number;
  lineTotal: number;
}

export interface Invoice {
  id: number;
  invoiceType: InvoiceType;
  invoiceNo: string;
  partnerId: number;
  partnerName: string;
  warehouseId: number;
  warehouseName: string;
  date: string;
  paymentMode: PaymentMode;
  paymentTermDays: number;
  dueDate: string;
  amountPaid: number;
  outstandingAmount: number;
  paymentStatus: InvoicePaymentStatus;
  status: InvoiceStatus;
  narration: string | null;
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
  status: InvoiceStatus;
  totalAmount: number;
}
