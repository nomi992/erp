import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { Product } from './product.types';

export async function getProducts(): Promise<Product[]> {
  const response = await apiClient.get<ApiResponse<Product[]>>('/api/products');
  return response.data.data ?? [];
}
