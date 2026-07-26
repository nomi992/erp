import { RightCode } from '../../core/auth/right-code';
import { PaymentDirection } from '../../core/partner-payments/partner-payment.models';

export interface PartnerPaymentConfig {
  direction: PaymentDirection;
  title: string;
  singularLabel: string;
  listRoute: string;
  formRoute: string;
  createRight: RightCode;
  submitRight: RightCode;
  approveRight: RightCode;
  rejectRight: RightCode;
}
