import { Component } from '@angular/core';
import { InvoiceForm } from '../../shared/invoices/invoice-form';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-sales-order-form',
  imports: [InvoiceForm],
  template: `<app-invoice-form [config]="config" />`,
})
export class SalesOrderForm {
  readonly config: InvoiceTypeConfig = {
    invoiceType: 'SalesOrder',
    title: 'Sales Orders',
    singularLabel: 'Sales Order',
    listRoute: '/sales-orders',
    formRoute: '/sales-orders',
    createRight: RightCode.SalesOrdersCreate,
    editRight: RightCode.SalesOrdersEdit,
    submitRight: RightCode.SalesOrdersSubmit,
    approveRight: RightCode.SalesOrdersApprove,
    rejectRight: RightCode.SalesOrdersReject,
    cancelRight: RightCode.SalesOrdersCancel,
    isOrder: true,
    isReturn: false,
    isPurchaseSide: false,
    referenceInvoiceType: null,
    referenceLabel: null,
  };
}
