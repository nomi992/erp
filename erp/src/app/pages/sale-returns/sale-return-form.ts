import { Component } from '@angular/core';
import { InvoiceForm } from '../../shared/invoices/invoice-form';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-sale-return-form',
  imports: [InvoiceForm],
  template: `<app-invoice-form [config]="config" />`,
})
export class SaleReturnForm {
  readonly config: InvoiceTypeConfig = {
    invoiceType: 'SaleReturn',
    title: 'Sale Returns',
    singularLabel: 'Sale Return',
    listRoute: '/sale-returns',
    formRoute: '/sale-returns',
    createRight: RightCode.SaleReturnsCreate,
    editRight: RightCode.SaleReturnsEdit,
    submitRight: RightCode.SaleReturnsSubmit,
    approveRight: RightCode.SaleReturnsApprove,
    rejectRight: RightCode.SaleReturnsReject,
    cancelRight: RightCode.SaleReturnsReject,
    isOrder: false,
    isReturn: true,
    isPurchaseSide: false,
    referenceInvoiceType: 'SalesInvoice',
    referenceLabel: 'Sales Invoice (being returned)',
  };
}
