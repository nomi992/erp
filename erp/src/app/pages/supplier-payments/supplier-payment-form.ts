import { Component } from '@angular/core';
import { PartnerPaymentForm } from '../../shared/partner-payments/partner-payment-form';
import { PartnerPaymentConfig } from '../../shared/partner-payments/partner-payment-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-supplier-payment-form',
  imports: [PartnerPaymentForm],
  template: `<app-partner-payment-form [config]="config" />`,
})
export class SupplierPaymentForm {
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
