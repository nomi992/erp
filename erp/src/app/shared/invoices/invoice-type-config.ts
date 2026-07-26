import { RightCode } from '../../core/auth/right-code';
import { InvoiceType } from '../../core/invoices/invoice.models';

export interface InvoiceTypeConfig {
  invoiceType: InvoiceType;
  title: string;
  singularLabel: string;
  listRoute: string;
  formRoute: string;
  createRight: RightCode;
  editRight: RightCode;
  submitRight: RightCode;
  approveRight: RightCode;
  rejectRight: RightCode;
  cancelRight?: RightCode;
  isOrder: boolean;
  isReturn: boolean;
  isPurchaseSide: boolean;
  referenceInvoiceType: InvoiceType | null;
  referenceLabel: string | null;
}
