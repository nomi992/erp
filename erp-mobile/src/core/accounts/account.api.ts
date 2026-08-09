import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { Account } from './account.types';

export async function getAccounts(): Promise<Account[]> {
  const response = await apiClient.get<ApiResponse<Account[]>>('/api/accounts');
  return response.data.data ?? [];
}
