import { Component } from '@angular/core';
import { InvoiceForm } from '../../shared/invoices/invoice-form';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-purchase-invoice-form',
  imports: [InvoiceForm],
  template: `<app-invoice-form [config]="config" />`,
})
export class PurchaseInvoiceForm {
  readonly config: InvoiceTypeConfig = {
    invoiceType: 'PurchaseInvoice',
    title: 'Purchase Invoices',
    singularLabel: 'Purchase Invoice',
    listRoute: '/purchase-invoices',
    formRoute: '/purchase-invoices',
    createRight: RightCode.PurchaseInvoicesCreate,
    editRight: RightCode.PurchaseInvoicesEdit,
    submitRight: RightCode.PurchaseInvoicesSubmit,
    approveRight: RightCode.PurchaseInvoicesApprove,
    rejectRight: RightCode.PurchaseInvoicesReject,
    cancelRight: RightCode.PurchaseInvoicesReject,
    isOrder: false,
    isReturn: false,
    isPurchaseSide: true,
    referenceInvoiceType: 'PurchaseOrder',
    referenceLabel: 'Purchase Order (optional)',
  };
}
