import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { ProductCategory, ProductCategoryRequest } from './product-category.models';

@Injectable({ providedIn: 'root' })
export class ProductCategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/productcategories`;

  getAll(): Observable<ApiResponse<ProductCategory[]>> {
    return this.http.get<ApiResponse<ProductCategory[]>>(this.baseUrl);
  }

  create(request: ProductCategoryRequest): Observable<ApiResponse<ProductCategory>> {
    return this.http.post<ApiResponse<ProductCategory>>(this.baseUrl, request);
  }

  update(id: number, request: ProductCategoryRequest): Observable<ApiResponse<ProductCategory>> {
    return this.http.put<ApiResponse<ProductCategory>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
