import { Component } from '@angular/core';
import { SimpleVoucherForm } from '../../shared/simple-vouchers/simple-voucher-form';
import { SimpleVoucherConfig } from '../../shared/simple-vouchers/simple-voucher-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-payment-voucher-form',
  imports: [SimpleVoucherForm],
  template: `<app-simple-voucher-form [config]="config" />`,
})
export class PaymentVoucherForm {
  readonly config: SimpleVoucherConfig = {
    voucherType: 'Payment',
    title: 'New Payment Voucher',
    postingAccountLabel: 'Paid To (Expense / Payable Account)',
    listRoute: '/vouchers',
    createRight: RightCode.VouchersCreate,
  };
}
