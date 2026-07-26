import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { UnitOfMeasure, UnitOfMeasureRequest } from './unit-of-measure.models';

@Injectable({ providedIn: 'root' })
export class UnitOfMeasureService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/unitsofmeasure`;

  getAll(): Observable<ApiResponse<UnitOfMeasure[]>> {
    return this.http.get<ApiResponse<UnitOfMeasure[]>>(this.baseUrl);
  }

  create(request: UnitOfMeasureRequest): Observable<ApiResponse<UnitOfMeasure>> {
    return this.http.post<ApiResponse<UnitOfMeasure>>(this.baseUrl, request);
  }

  update(id: number, request: UnitOfMeasureRequest): Observable<ApiResponse<UnitOfMeasure>> {
    return this.http.put<ApiResponse<UnitOfMeasure>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
