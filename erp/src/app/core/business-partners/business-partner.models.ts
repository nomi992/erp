export type PartnerType = 'Supplier' | 'Customer' | 'Both';

export interface BusinessPartner {
  id: number;
  partnerType: PartnerType;
  code: string;
  name: string;
  contactPerson: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  defaultPaymentTermDays: number;
  creditLimit: number | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface BusinessPartnerRequest {
  partnerType: PartnerType;
  code: string;
  name: string;
  contactPerson: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  defaultPaymentTermDays: number;
  creditLimit: number | null;
}
