import { RightCode } from '../../core/auth/right-code';
import { VoucherType } from '../../core/vouchers/voucher.models';

/**
 * Drives the simplified Payment/Receipt Voucher form (`SimpleVoucherForm`).
 *
 * The user never sees "debit"/"credit" — they pick one posting account (the
 * expense/income/payable/receivable side), optionally flag the other side as
 * a bank account (vs. cash), enter an amount and narration. The component
 * builds the balanced two-line `CreateVoucherRequest` itself.
 */
export interface SimpleVoucherConfig {
  /** Must be 'Payment' or 'Receipt' — the two voucher types this form supports. */
  voucherType: VoucherType;
  /** Page heading, e.g. "New Payment Voucher". */
  title: string;
  /** Label for the single account the user picks, e.g. "Paid To (Expense / Payable Account)". */
  postingAccountLabel: string;
  /** Route to return to after saving (the generic voucher list, filtered elsewhere by type if desired). */
  listRoute: string;
  /** Right required to save — reuses the standard voucher rights, no new right codes needed. */
  createRight: RightCode;
}
