import { Component } from '@angular/core';
import { InvoiceList } from '../../shared/invoices/invoice-list';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-sale-return-list',
  imports: [InvoiceList],
  template: `<app-invoice-list [config]="config" />`,
})
export class SaleReturnList {
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
