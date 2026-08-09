import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { PagedResult } from '../models/paged-result';
import type { StockLedgerEntry, StockOnHand } from './stock.types';

export interface StockOnHandFilter {
  warehouseId?: number;
  lowStockOnly?: boolean;
}

export interface StockLedgerFilter {
  warehouseId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export async function getStockOnHand(filter: StockOnHandFilter = {}): Promise<StockOnHand[]> {
  const params: Record<string, string> = {};
  if (filter.warehouseId) {
    params.warehouseId = String(filter.warehouseId);
  }
  if (filter.lowStockOnly) {
    params.lowStockOnly = String(filter.lowStockOnly);
  }
  const response = await apiClient.get<ApiResponse<StockOnHand[]>>('/api/stockledger/on-hand', { params });
  return response.data.data ?? [];
}

const DEFAULT_PAGE_SIZE = 25;

export async function getStockLedger(filter: StockLedgerFilter = {}): Promise<PagedResult<StockLedgerEntry>> {
  const pageSize = filter.pageSize ?? DEFAULT_PAGE_SIZE;
  const params: Record<string, string> = {
    pageNumber: String(filter.pageNumber ?? 1),
    pageSize: String(pageSize),
  };
  if (filter.warehouseId) {
    params.warehouseId = String(filter.warehouseId);
  }
  const response = await apiClient.get<ApiResponse<PagedResult<StockLedgerEntry>>>('/api/stockledger', { params });
  return response.data.data ?? { items: [], totalCount: 0, pageNumber: filter.pageNumber ?? 1, pageSize };
}
