import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Branch, BranchRequest } from './branch.models';

@Injectable({ providedIn: 'root' })
export class BranchService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/branches`;

  getAll(tenantId?: number | null): Observable<ApiResponse<Branch[]>> {
    const params = tenantId != null ? new HttpParams().set('tenantId', tenantId) : undefined;
    return this.http.get<ApiResponse<Branch[]>>(this.baseUrl, { params });
  }

  create(request: BranchRequest): Observable<ApiResponse<Branch>> {
    return this.http.post<ApiResponse<Branch>>(this.baseUrl, request);
  }

  update(id: number, request: BranchRequest): Observable<ApiResponse<Branch>> {
    return this.http.put<ApiResponse<Branch>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
