// Subset of erp/src/app/core/business-partners/business-partner.models.ts needed for the
// Sales Invoice customer picker.
export type PartnerType = 'Supplier' | 'Customer' | 'Both';

export interface BusinessPartner {
  id: number;
  partnerType: PartnerType;
  code: string;
  name: string;
  defaultPaymentTermDays: number;
  isActive: boolean;
}
