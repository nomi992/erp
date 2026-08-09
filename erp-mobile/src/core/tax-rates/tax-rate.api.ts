import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { TaxRate } from './tax-rate.types';

export async function getTaxRates(): Promise<TaxRate[]> {
  const response = await apiClient.get<ApiResponse<TaxRate[]>>('/api/taxrates');
  return response.data.data ?? [];
}
