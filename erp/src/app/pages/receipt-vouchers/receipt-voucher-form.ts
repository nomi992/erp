import { Component } from '@angular/core';
import { SimpleVoucherForm } from '../../shared/simple-vouchers/simple-voucher-form';
import { SimpleVoucherConfig } from '../../shared/simple-vouchers/simple-voucher-config';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-receipt-voucher-form',
  imports: [SimpleVoucherForm],
  template: `<app-simple-voucher-form [config]="config" />`,
})
export class ReceiptVoucherForm {
  readonly config: SimpleVoucherConfig = {
    voucherType: 'Receipt',
    title: 'New Receipt Voucher',
    postingAccountLabel: 'Received From (Income / Receivable Account)',
    listRoute: '/vouchers',
    createRight: RightCode.VouchersCreate,
  };
}
