import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { PagedResult } from '../models/paged-result';
import type { Invoice, InvoiceListItem, InvoiceRequest } from './invoice.types';

const DEFAULT_PAGE_SIZE = 25;

export async function getSalesInvoices(pageNumber = 1): Promise<PagedResult<InvoiceListItem>> {
  const response = await apiClient.get<ApiResponse<PagedResult<InvoiceListItem>>>('/api/invoices', {
    params: { invoiceType: 'SalesInvoice', pageNumber, pageSize: DEFAULT_PAGE_SIZE, sortBy: 'date', sortDirection: 'desc' },
  });
  return response.data.data ?? { items: [], totalCount: 0, pageNumber, pageSize: DEFAULT_PAGE_SIZE };
}

export async function getInvoiceById(id: number): Promise<Invoice> {
  const response = await apiClient.get<ApiResponse<Invoice>>(`/api/invoices/${id}`);
  if (!response.data.data) {
    throw new Error(response.data.message || 'Invoice not found.');
  }
  return response.data.data;
}

export async function createInvoice(request: InvoiceRequest): Promise<Invoice> {
  const response = await apiClient.post<ApiResponse<Invoice>>('/api/invoices', request);
  if (!response.data.data) {
    throw new Error(response.data.message || 'Unable to create invoice.');
  }
  return response.data.data;
}
