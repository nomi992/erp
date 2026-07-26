import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { RecurringVoucherTemplate, RecurringVoucherTemplateRequest } from './voucher.models';

@Injectable({ providedIn: 'root' })
export class RecurringVoucherTemplateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/recurringvouchertemplates`;

  getAll(): Observable<ApiResponse<RecurringVoucherTemplate[]>> {
    return this.http.get<ApiResponse<RecurringVoucherTemplate[]>>(this.baseUrl);
  }

  getById(id: number): Observable<ApiResponse<RecurringVoucherTemplate>> {
    return this.http.get<ApiResponse<RecurringVoucherTemplate>>(`${this.baseUrl}/${id}`);
  }

  create(request: RecurringVoucherTemplateRequest): Observable<ApiResponse<RecurringVoucherTemplate>> {
    return this.http.post<ApiResponse<RecurringVoucherTemplate>>(this.baseUrl, request);
  }

  update(id: number, request: RecurringVoucherTemplateRequest): Observable<ApiResponse<RecurringVoucherTemplate>> {
    return this.http.put<ApiResponse<RecurringVoucherTemplate>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }

  generateNow(id: number): Observable<ApiResponse<{ id: number; voucherNo: string }>> {
    return this.http.post<ApiResponse<{ id: number; voucherNo: string }>>(`${this.baseUrl}/${id}/generate-now`, {});
  }
}
