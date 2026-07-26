import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { ReportSchedule, ReportScheduleRequest } from './report-schedule.models';

@Injectable({ providedIn: 'root' })
export class ReportScheduleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/reportschedules`;

  getAll(): Observable<ApiResponse<ReportSchedule[]>> {
    return this.http.get<ApiResponse<ReportSchedule[]>>(this.baseUrl);
  }

  create(request: ReportScheduleRequest): Observable<ApiResponse<ReportSchedule>> {
    return this.http.post<ApiResponse<ReportSchedule>>(this.baseUrl, request);
  }

  update(id: number, request: ReportScheduleRequest): Observable<ApiResponse<ReportSchedule>> {
    return this.http.put<ApiResponse<ReportSchedule>>(`${this.baseUrl}/${id}`, request);
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

  runNow(id: number): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/${id}/run-now`, {}, { responseType: 'blob' });
  }
}
