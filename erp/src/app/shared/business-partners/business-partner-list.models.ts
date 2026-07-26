import { RightCode } from '../../core/auth/right-code';
import { PartnerType } from '../../core/business-partners/business-partner.models';

export interface BusinessPartnerListConfig {
  listPartnerType: 'Supplier' | 'Customer';
  allowedPartnerTypes: PartnerType[];
  title: string;
  createRight: RightCode;
  editRight: RightCode;
}
