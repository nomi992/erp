import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';
import { PartnerPayment, PartnerPaymentListItem, PartnerPaymentRequest, PartnerPaymentStatus, PaymentDirection } from './partner-payment.models';

export interface PartnerPaymentFilter {
  direction?: PaymentDirection;
  partnerId?: number;
  status?: PartnerPaymentStatus;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class PartnerPaymentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/partnerpayments`;

  getAll(filter: PartnerPaymentFilter = {}): Observable<ApiResponse<PagedResult<PartnerPaymentListItem>>> {
    const params: Record<string, string> = {};
    if (filter.direction) params['direction'] = filter.direction;
    if (filter.partnerId) params['partnerId'] = String(filter.partnerId);
    if (filter.status) params['status'] = filter.status;
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    params['pageNumber'] = String(filter.pageNumber ?? 1);
    params['pageSize'] = String(filter.pageSize ?? 25);

    return this.http.get<ApiResponse<PagedResult<PartnerPaymentListItem>>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<PartnerPayment>> {
    return this.http.get<ApiResponse<PartnerPayment>>(`${this.baseUrl}/${id}`);
  }

  create(request: PartnerPaymentRequest): Observable<ApiResponse<PartnerPayment>> {
    return this.http.post<ApiResponse<PartnerPayment>>(this.baseUrl, request);
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
}
