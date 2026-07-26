import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateVoucherRequest, Voucher, VoucherAttachment, VoucherListItem, VoucherStatus, VoucherType } from './voucher.models';

export interface VoucherFilter {
  type?: VoucherType;
  status?: VoucherStatus;
  from?: string;
  to?: string;
}

@Injectable({ providedIn: 'root' })
export class VoucherService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/vouchers`;

  getAll(filter: VoucherFilter = {}): Observable<ApiResponse<VoucherListItem[]>> {
    const params: Record<string, string> = {};
    if (filter.type) params['type'] = filter.type;
    if (filter.status) params['status'] = filter.status;
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;

    return this.http.get<ApiResponse<VoucherListItem[]>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<Voucher>> {
    return this.http.get<ApiResponse<Voucher>>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateVoucherRequest): Observable<ApiResponse<Voucher>> {
    return this.http.post<ApiResponse<Voucher>>(this.baseUrl, request);
  }

  update(id: number, request: CreateVoucherRequest): Observable<ApiResponse<Voucher>> {
    return this.http.put<ApiResponse<Voucher>>(`${this.baseUrl}/${id}`, request);
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

  reverse(id: number): Observable<ApiResponse<Voucher>> {
    return this.http.post<ApiResponse<Voucher>>(`${this.baseUrl}/${id}/reverse`, {});
  }

  uploadAttachment(id: number, file: File): Observable<ApiResponse<VoucherAttachment>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<VoucherAttachment>>(`${this.baseUrl}/${id}/attachments`, formData);
  }

  downloadAttachment(voucherId: number, attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${voucherId}/attachments/${attachmentId}/download`, { responseType: 'blob' });
  }

  deleteAttachment(voucherId: number, attachmentId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${voucherId}/attachments/${attachmentId}`);
  }
}
