import { Component } from '@angular/core';
import { InvoiceForm } from '../../shared/invoices/invoice-form';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-sales-invoice-form',
  imports: [InvoiceForm],
  template: `<app-invoice-form [config]="config" />`,
})
export class SalesInvoiceForm {
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
    printable: true,
  };
}
