import { Component } from '@angular/core';
import { InvoiceForm } from '../../shared/invoices/invoice-form';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-purchase-return-form',
  imports: [InvoiceForm],
  template: `<app-invoice-form [config]="config" />`,
})
export class PurchaseReturnForm {
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
