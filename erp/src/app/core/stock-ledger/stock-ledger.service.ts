import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';
import { PartnerAging, StockLedgerEntry, StockOnHand } from './stock-ledger.models';

export interface StockLedgerFilter {
  productVariantId?: number;
  warehouseId?: number;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface StockOnHandFilter {
  warehouseId?: number;
  productVariantId?: number;
  lowStockOnly?: boolean;
}

@Injectable({ providedIn: 'root' })
export class StockLedgerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/stockledger`;

  getLedger(filter: StockLedgerFilter = {}): Observable<ApiResponse<PagedResult<StockLedgerEntry>>> {
    const params: Record<string, string> = {};
    if (filter.productVariantId) params['productVariantId'] = String(filter.productVariantId);
    if (filter.warehouseId) params['warehouseId'] = String(filter.warehouseId);
    if (filter.from) params['from'] = filter.from;
    if (filter.to) params['to'] = filter.to;
    params['pageNumber'] = String(filter.pageNumber ?? 1);
    params['pageSize'] = String(filter.pageSize ?? 25);

    return this.http.get<ApiResponse<PagedResult<StockLedgerEntry>>>(this.baseUrl, { params });
  }

  getOnHand(filter: StockOnHandFilter = {}): Observable<ApiResponse<StockOnHand[]>> {
    const params: Record<string, string> = {};
    if (filter.warehouseId) params['warehouseId'] = String(filter.warehouseId);
    if (filter.productVariantId) params['productVariantId'] = String(filter.productVariantId);
    if (filter.lowStockOnly) params['lowStockOnly'] = String(filter.lowStockOnly);

    return this.http.get<ApiResponse<StockOnHand[]>>(`${this.baseUrl}/on-hand`, { params });
  }

  getAccountsReceivableAging(): Observable<ApiResponse<PartnerAging[]>> {
    return this.http.get<ApiResponse<PartnerAging[]>>(`${this.baseUrl}/ar-aging`);
  }

  getAccountsPayableAging(): Observable<ApiResponse<PartnerAging[]>> {
    return this.http.get<ApiResponse<PartnerAging[]>>(`${this.baseUrl}/ap-aging`);
  }
}
