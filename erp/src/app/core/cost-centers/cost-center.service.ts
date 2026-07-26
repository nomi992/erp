import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CostCenter, CostCenterRequest } from './cost-center.models';

@Injectable({ providedIn: 'root' })
export class CostCenterService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/costcenters`;

  getAll(): Observable<ApiResponse<CostCenter[]>> {
    return this.http.get<ApiResponse<CostCenter[]>>(this.baseUrl);
  }

  create(request: CostCenterRequest): Observable<ApiResponse<CostCenter>> {
    return this.http.post<ApiResponse<CostCenter>>(this.baseUrl, request);
  }

  update(id: number, request: CostCenterRequest): Observable<ApiResponse<CostCenter>> {
    return this.http.put<ApiResponse<CostCenter>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}`);
  }
}
