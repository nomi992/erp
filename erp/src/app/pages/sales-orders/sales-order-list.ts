import { Component } from '@angular/core';
import { InvoiceList } from '../../shared/invoices/invoice-list';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-sales-order-list',
  imports: [InvoiceList],
  template: `<app-invoice-list [config]="config" />`,
})
export class SalesOrderList {
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
