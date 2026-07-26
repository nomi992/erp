import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { TaxRate, TaxRateRequest } from './tax-rate.models';

@Injectable({ providedIn: 'root' })
export class TaxRateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/taxrates`;

  getAll(): Observable<ApiResponse<TaxRate[]>> {
    return this.http.get<ApiResponse<TaxRate[]>>(this.baseUrl);
  }

  create(request: TaxRateRequest): Observable<ApiResponse<TaxRate>> {
    return this.http.post<ApiResponse<TaxRate>>(this.baseUrl, request);
  }

  update(id: number, request: TaxRateRequest): Observable<ApiResponse<TaxRate>> {
    return this.http.put<ApiResponse<TaxRate>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
