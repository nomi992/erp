import { Component } from '@angular/core';
import { InvoiceList } from '../../shared/invoices/invoice-list';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-sales-invoice-list',
  imports: [InvoiceList],
  template: `<app-invoice-list [config]="config" />`,
})
export class SalesInvoiceList {
  readonly config: InvoiceTypeConfig = {
    invoiceType: 'SalesInvoice',
    title: 'Sales Invoices',
    singularLabel: 'Sales Invoice',
    listRoute: '/sales-invoices',
    formRoute: '/sales-invoices',
    createRight: RightCode.SalesInvoicesCreate,
    editRight: RightCode.SalesInvoicesEdit,
    submitRight: RightCode.SalesInvoicesSubmit,
    approveRight: RightCode.SalesInvoicesApprove,
    rejectRight: RightCode.SalesInvoicesReject,
    cancelRight: RightCode.SalesInvoicesReject,
    isOrder: false,
    isReturn: false,
    isPurchaseSide: false,
    referenceInvoiceType: 'SalesOrder',
    referenceLabel: 'Sales Order (optional)',
  };
}
