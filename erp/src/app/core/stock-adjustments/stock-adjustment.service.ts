import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';
import {
  AdjustmentReasonCode,
  StockAdjustment,
  StockAdjustmentListItem,
  StockAdjustmentRequest,
  StockAdjustmentStatus,
} from './stock-adjustment.models';

export interface StockAdjustmentFilter {
  reasonCode?: AdjustmentReasonCode;
  status?: StockAdjustmentStatus;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class StockAdjustmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/stockadjustments`;

  getAll(filter: StockAdjustmentFilter = {}): Observable<ApiResponse<PagedResult<StockAdjustmentListItem>>> {
    const params: Record<string, string> = {};
    if (filter.reasonCode) params['reasonCode'] = filter.reasonCode;
    if (filter.status) params['status'] = filter.status;
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    params['pageNumber'] = String(filter.pageNumber ?? 1);
    params['pageSize'] = String(filter.pageSize ?? 25);

    return this.http.get<ApiResponse<PagedResult<StockAdjustmentListItem>>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<StockAdjustment>> {
    return this.http.get<ApiResponse<StockAdjustment>>(`${this.baseUrl}/${id}`);
  }

  create(request: StockAdjustmentRequest): Observable<ApiResponse<StockAdjustment>> {
    return this.http.post<ApiResponse<StockAdjustment>>(this.baseUrl, request);
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
