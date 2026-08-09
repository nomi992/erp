import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { Branch } from './branch.types';

export async function getBranches(): Promise<Branch[]> {
  const response = await apiClient.get<ApiResponse<Branch[]>>('/api/branches');
  return response.data.data ?? [];
}
