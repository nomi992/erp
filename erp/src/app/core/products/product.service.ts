import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  Product,
  ProductRequest,
  ProductVariant,
  ProductVariantPrice,
  ProductVariantPriceRequest,
  ProductVariantRequest,
  UOMConversion,
  UOMConversionRequest,
} from './product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/products`;

  getAll(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(this.baseUrl);
  }

  getById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.baseUrl}/${id}`);
  }

  create(request: ProductRequest): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(this.baseUrl, request);
  }

  update(id: number, request: ProductRequest): Observable<ApiResponse<Product>> {
    return this.http.put<ApiResponse<Product>>(`${this.baseUrl}/${id}`, request);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${id}/deactivate`, {});
  }

  addVariant(productId: number, request: ProductVariantRequest): Observable<ApiResponse<ProductVariant>> {
    return this.http.post<ApiResponse<ProductVariant>>(`${this.baseUrl}/${productId}/variants`, request);
  }

  updateVariant(productId: number, variantId: number, request: ProductVariantRequest): Observable<ApiResponse<ProductVariant>> {
    return this.http.put<ApiResponse<ProductVariant>>(`${this.baseUrl}/${productId}/variants/${variantId}`, request);
  }

  activateVariant(productId: number, variantId: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${productId}/variants/${variantId}/activate`, {});
  }

  deactivateVariant(productId: number, variantId: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.baseUrl}/${productId}/variants/${variantId}/deactivate`, {});
  }

  setVariantPrices(productId: number, variantId: number, prices: ProductVariantPriceRequest[]): Observable<ApiResponse<ProductVariantPrice[]>> {
    return this.http.put<ApiResponse<ProductVariantPrice[]>>(`${this.baseUrl}/${productId}/variants/${variantId}/prices`, prices);
  }

  addConversion(productId: number, request: UOMConversionRequest): Observable<ApiResponse<UOMConversion>> {
    return this.http.post<ApiResponse<UOMConversion>>(`${this.baseUrl}/${productId}/conversions`, request);
  }

  updateConversion(productId: number, conversionId: number, request: UOMConversionRequest): Observable<ApiResponse<UOMConversion>> {
    return this.http.put<ApiResponse<UOMConversion>>(`${this.baseUrl}/${productId}/conversions/${conversionId}`, request);
  }

  deleteConversion(productId: number, conversionId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${productId}/conversions/${conversionId}`);
  }
}
