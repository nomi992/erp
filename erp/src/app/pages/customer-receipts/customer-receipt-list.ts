import { Component } from '@angular/core';
import { PartnerPaymentList } from '../../shared/partner-payments/partner-payment-list';
import { PartnerPaymentConfig } from '../../shared/partner-payments/partner-payment-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-customer-receipt-list',
  imports: [PartnerPaymentList],
  template: `<app-partner-payment-list [config]="config" />`,
})
export class CustomerReceiptList {
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
