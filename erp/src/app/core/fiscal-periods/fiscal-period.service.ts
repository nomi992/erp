import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { FiscalPeriod, FiscalPeriodRequest } from './fiscal-period.models';

@Injectable({ providedIn: 'root' })
export class FiscalPeriodService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/fiscalperiods`;

  getAll(): Observable<ApiResponse<FiscalPeriod[]>> {
    return this.http.get<ApiResponse<FiscalPeriod[]>>(this.baseUrl);
  }

  create(request: FiscalPeriodRequest): Observable<ApiResponse<FiscalPeriod>> {
    return this.http.post<ApiResponse<FiscalPeriod>>(this.baseUrl, request);
  }

  close(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/close`, {});
  }

  open(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/open`, {});
  }
}
