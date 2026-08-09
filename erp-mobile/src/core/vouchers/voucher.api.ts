import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { CreateVoucherRequest, Voucher, VoucherListItem, VoucherStatus, VoucherType } from './voucher.types';

export interface VoucherFilter {
  type?: VoucherType;
  status?: VoucherStatus;
}

export async function getVouchers(filter: VoucherFilter = {}): Promise<VoucherListItem[]> {
  const params: Record<string, string> = {};
  if (filter.type) {
    params.type = filter.type;
  }
  if (filter.status) {
    params.status = filter.status;
  }
  const response = await apiClient.get<ApiResponse<VoucherListItem[]>>('/api/vouchers', { params });
  return response.data.data ?? [];
}

export async function getVoucherById(id: number): Promise<Voucher> {
  const response = await apiClient.get<ApiResponse<Voucher>>(`/api/vouchers/${id}`);
  if (!response.data.data) {
    throw new Error(response.data.message || 'Voucher not found.');
  }
  return response.data.data;
}

export async function createVoucher(request: CreateVoucherRequest): Promise<Voucher> {
  const response = await apiClient.post<ApiResponse<Voucher>>('/api/vouchers', request);
  if (!response.data.data) {
    throw new Error(response.data.message || 'Unable to create voucher.');
  }
  return response.data.data;
}
