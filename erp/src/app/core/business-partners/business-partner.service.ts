import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { BusinessPartner, BusinessPartnerRequest, PartnerType } from './business-partner.models';

@Injectable({ providedIn: 'root' })
export class BusinessPartnerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/businesspartners`;

  getAll(partnerType?: PartnerType): Observable<ApiResponse<BusinessPartner[]>> {
    const params: Record<string, string> = {};
    if (partnerType) params['partnerType'] = partnerType;
    return this.http.get<ApiResponse<BusinessPartner[]>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<BusinessPartner>> {
    return this.http.get<ApiResponse<BusinessPartner>>(`${this.baseUrl}/${id}`);
  }

  create(request: BusinessPartnerRequest): Observable<ApiResponse<BusinessPartner>> {
    return this.http.post<ApiResponse<BusinessPartner>>(this.baseUrl, request);
  }

  update(id: number, request: BusinessPartnerRequest): Observable<ApiResponse<BusinessPartner>> {
    return this.http.put<ApiResponse<BusinessPartner>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
