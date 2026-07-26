import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { RightGroup } from './right.models';

@Injectable({ providedIn: 'root' })
export class RightsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/rights`;

  getAll(): Observable<ApiResponse<RightGroup[]>> {
    return this.http.get<ApiResponse<RightGroup[]>>(this.baseUrl);
  }
}
