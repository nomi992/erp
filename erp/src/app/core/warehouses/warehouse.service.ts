import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Warehouse, WarehouseRequest } from './warehouse.models';

@Injectable({ providedIn: 'root' })
export class WarehouseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/warehouses`;

  getAll(): Observable<ApiResponse<Warehouse[]>> {
    return this.http.get<ApiResponse<Warehouse[]>>(this.baseUrl);
  }

  create(request: WarehouseRequest): Observable<ApiResponse<Warehouse>> {
    return this.http.post<ApiResponse<Warehouse>>(this.baseUrl, request);
  }

  update(id: number, request: WarehouseRequest): Observable<ApiResponse<Warehouse>> {
    return this.http.put<ApiResponse<Warehouse>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
