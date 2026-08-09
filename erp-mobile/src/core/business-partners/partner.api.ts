import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { BusinessPartner, PartnerType } from './partner.types';

export async function getBusinessPartners(partnerType?: PartnerType): Promise<BusinessPartner[]> {
  const params: Record<string, string> = {};
  if (partnerType) {
    params.partnerType = partnerType;
  }
  const response = await apiClient.get<ApiResponse<BusinessPartner[]>>('/api/businesspartners', { params });
  return response.data.data ?? [];
}
