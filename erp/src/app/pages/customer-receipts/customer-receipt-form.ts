import { Component } from '@angular/core';
import { PartnerPaymentForm } from '../../shared/partner-payments/partner-payment-form';
import { PartnerPaymentConfig } from '../../shared/partner-payments/partner-payment-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-customer-receipt-form',
  imports: [PartnerPaymentForm],
  template: `<app-partner-payment-form [config]="config" />`,
})
export class CustomerReceiptForm {
  readonly config: PartnerPaymentConfig = {
    direction: 'CustomerReceipt',
    title: 'Customer Receipts',
    singularLabel: 'Customer Receipt',
    listRoute: '/customer-receipts',
    formRoute: '/customer-receipts',
    createRight: RightCode.CustomerReceiptsCreate,
    submitRight: RightCode.CustomerReceiptsSubmit,
    approveRight: RightCode.CustomerReceiptsApprove,
    rejectRight: RightCode.CustomerReceiptsReject,
  };
}
