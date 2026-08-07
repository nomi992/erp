import { Component } from '@angular/core';
import { InvoiceList } from '../../shared/invoices/invoice-list';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-purchase-invoice-list',
  imports: [InvoiceList],
  template: `<app-invoice-list [config]="config" />`,
})
export class PurchaseInvoiceList {
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
    printable: true,
  };
}
