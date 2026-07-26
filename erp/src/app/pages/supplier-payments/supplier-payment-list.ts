import { Component } from '@angular/core';
import { PartnerPaymentList } from '../../shared/partner-payments/partner-payment-list';
import { PartnerPaymentConfig } from '../../shared/partner-payments/partner-payment-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-supplier-payment-list',
  imports: [PartnerPaymentList],
  template: `<app-partner-payment-list [config]="config" />`,
})
export class SupplierPaymentList {
  readonly config: PartnerPaymentConfig = {
    direction: 'SupplierPayment',
    title: 'Supplier Payments',
    singularLabel: 'Supplier Payment',
    listRoute: '/supplier-payments',
    formRoute: '/supplier-payments',
    createRight: RightCode.SupplierPaymentsCreate,
    submitRight: RightCode.SupplierPaymentsSubmit,
    approveRight: RightCode.SupplierPaymentsApprove,
    rejectRight: RightCode.SupplierPaymentsReject,
  };
}
