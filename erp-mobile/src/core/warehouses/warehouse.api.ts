import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { Warehouse } from './warehouse.types';

export async function getWarehouses(): Promise<Warehouse[]> {
  const response = await apiClient.get<ApiResponse<Warehouse[]>>('/api/warehouses');
  return response.data.data ?? [];
}
