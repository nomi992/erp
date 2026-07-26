import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Budget, BudgetRequest } from './budget.models';

@Injectable({ providedIn: 'root' })
export class BudgetService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/budgets`;

  getAll(year?: number, month?: number): Observable<ApiResponse<Budget[]>> {
    const params: Record<string, string> = {};
    if (year) params['year'] = String(year);
    if (month) params['month'] = String(month);

    return this.http.get<ApiResponse<Budget[]>>(this.baseUrl, { params });
  }

  create(request: BudgetRequest): Observable<ApiResponse<Budget>> {
    return this.http.post<ApiResponse<Budget>>(this.baseUrl, request);
  }

  update(id: number, request: BudgetRequest): Observable<ApiResponse<Budget>> {
    return this.http.put<ApiResponse<Budget>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}`);
  }
}
