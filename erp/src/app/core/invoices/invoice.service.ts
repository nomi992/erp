import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';
import { Invoice, InvoiceListItem, InvoiceRequest, InvoiceStatus, InvoiceType } from './invoice.models';

export interface InvoiceFilter {
  invoiceType?: InvoiceType;
  partnerId?: number;
  status?: InvoiceStatus;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/invoices`;

  getAll(filter: InvoiceFilter = {}): Observable<ApiResponse<PagedResult<InvoiceListItem>>> {
    const params: Record<string, string> = {};
    if (filter.invoiceType) params['invoiceType'] = filter.invoiceType;
    if (filter.partnerId) params['partnerId'] = String(filter.partnerId);
    if (filter.status) params['status'] = filter.status;
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    params['pageNumber'] = String(filter.pageNumber ?? 1);
    params['pageSize'] = String(filter.pageSize ?? 25);

    return this.http.get<ApiResponse<PagedResult<InvoiceListItem>>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<Invoice>> {
    return this.http.get<ApiResponse<Invoice>>(`${this.baseUrl}/${id}`);
  }

  create(request: InvoiceRequest): Observable<ApiResponse<Invoice>> {
    return this.http.post<ApiResponse<Invoice>>(this.baseUrl, request);
  }

  update(id: number, request: InvoiceRequest): Observable<ApiResponse<Invoice>> {
    return this.http.put<ApiResponse<Invoice>>(`${this.baseUrl}/${id}`, request);
  }

  submit(id: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/${id}/submit`, {});
  }

  approve(id: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/${id}/reject`, {});
  }

  cancel(id: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
