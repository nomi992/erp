import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';
import {
  StockTransfer,
  StockTransferListItem,
  StockTransferReceiveRequest,
  StockTransferRequest,
  StockTransferStatus,
} from './stock-transfer.models';

export interface StockTransferFilter {
  status?: StockTransferStatus;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class StockTransferService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/stocktransfers`;

  getAll(filter: StockTransferFilter = {}): Observable<ApiResponse<PagedResult<StockTransferListItem>>> {
    const params: Record<string, string> = {};
    if (filter.status) params['status'] = filter.status;
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    params['pageNumber'] = String(filter.pageNumber ?? 1);
    params['pageSize'] = String(filter.pageSize ?? 25);

    return this.http.get<ApiResponse<PagedResult<StockTransferListItem>>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<StockTransfer>> {
    return this.http.get<ApiResponse<StockTransfer>>(`${this.baseUrl}/${id}`);
  }

  create(request: StockTransferRequest): Observable<ApiResponse<StockTransfer>> {
    return this.http.post<ApiResponse<StockTransfer>>(this.baseUrl, request);
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

  receive(id: number, request: StockTransferReceiveRequest): Observable<ApiResponse<StockTransfer>> {
    return this.http.post<ApiResponse<StockTransfer>>(`${this.baseUrl}/${id}/receive`, request);
  }
}
