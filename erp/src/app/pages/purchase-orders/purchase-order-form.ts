import { Component } from '@angular/core';
import { InvoiceForm } from '../../shared/invoices/invoice-form';
import { InvoiceTypeConfig } from '../../shared/invoices/invoice-type-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-purchase-order-form',
  imports: [InvoiceForm],
  template: `<app-invoice-form [config]="config" />`,
})
export class PurchaseOrderForm {
  readonly config: InvoiceTypeConfig = {
    invoiceType: 'PurchaseOrder',
    title: 'Purchase Orders',
    singularLabel: 'Purchase Order',
    listRoute: '/purchase-orders',
    formRoute: '/purchase-orders',
    createRight: RightCode.PurchaseOrdersCreate,
    editRight: RightCode.PurchaseOrdersEdit,
    submitRight: RightCode.PurchaseOrdersSubmit,
    approveRight: RightCode.PurchaseOrdersApprove,
    rejectRight: RightCode.PurchaseOrdersReject,
    cancelRight: RightCode.PurchaseOrdersCancel,
    isOrder: true,
    isReturn: false,
    isPurchaseSide: true,
    referenceInvoiceType: null,
    referenceLabel: null,
  };
}
