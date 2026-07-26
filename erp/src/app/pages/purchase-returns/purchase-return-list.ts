import { Component } from '@angular/core';
import { InvoiceList } from '../../shared/invoices/invoice-list';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-purchase-return-list',
  imports: [InvoiceList],
  template: `<app-invoice-list [config]="config" />`,
})
export class PurchaseReturnList {
  readonly config: InvoiceTypeConfig = {
    invoiceType: 'PurchaseReturn',
    title: 'Purchase Returns',
    singularLabel: 'Purchase Return',
    listRoute: '/purchase-returns',
    formRoute: '/purchase-returns',
    createRight: RightCode.PurchaseReturnsCreate,
    editRight: RightCode.PurchaseReturnsEdit,
    submitRight: RightCode.PurchaseReturnsSubmit,
    approveRight: RightCode.PurchaseReturnsApprove,
    rejectRight: RightCode.PurchaseReturnsReject,
    cancelRight: RightCode.PurchaseReturnsReject,
    isOrder: false,
    isReturn: true,
    isPurchaseSide: true,
    referenceInvoiceType: 'PurchaseInvoice',
    referenceLabel: 'Purchase Invoice (being returned)',
  };
}
