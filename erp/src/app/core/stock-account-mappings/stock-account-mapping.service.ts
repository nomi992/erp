import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { StockAccountMapping, StockAccountMappingRequest } from './stock-account-mapping.models';

@Injectable({ providedIn: 'root' })
export class StockAccountMappingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/stockaccountmappings`;

  getAll(): Observable<ApiResponse<StockAccountMapping[]>> {
    return this.http.get<ApiResponse<StockAccountMapping[]>>(this.baseUrl);
  }

  create(request: StockAccountMappingRequest): Observable<ApiResponse<StockAccountMapping>> {
    return this.http.post<ApiResponse<StockAccountMapping>>(this.baseUrl, request);
  }

  update(id: number, request: StockAccountMappingRequest): Observable<ApiResponse<StockAccountMapping>> {
    return this.http.put<ApiResponse<StockAccountMapping>>(`${this.baseUrl}/${id}`, request);
  }
}
