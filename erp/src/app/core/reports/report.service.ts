import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { SubLedgerEntry, SubLedgerType } from '../ledgers/ledger.models';
import {
  BalanceSheet,
  BudgetVsActualRow,
  CashFlow,
  DayBookEntry,
  ProfitLoss,
  TrialBalance,
} from './report.models';

export type ExportFormat = 'pdf' | 'excel';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/reports`;

  getTrialBalance(asOf?: string, costCenterId?: number): Observable<ApiResponse<TrialBalance>> {
    return this.http.get<ApiResponse<TrialBalance>>(`${this.baseUrl}/trial-balance`, {
      params: this.buildParams({ asOf, costCenterId }),
    });
  }

  getProfitLoss(from: string, to: string, compare = false, costCenterId?: number): Observable<ApiResponse<ProfitLoss>> {
    return this.http.get<ApiResponse<ProfitLoss>>(`${this.baseUrl}/profit-loss`, {
      params: this.buildParams({ from, to, compare, costCenterId }),
    });
  }

  getBalanceSheet(asOf?: string, costCenterId?: number): Observable<ApiResponse<BalanceSheet>> {
    return this.http.get<ApiResponse<BalanceSheet>>(`${this.baseUrl}/balance-sheet`, {
      params: this.buildParams({ asOf, costCenterId }),
    });
  }

  getCashFlow(from: string, to: string): Observable<ApiResponse<CashFlow>> {
    return this.http.get<ApiResponse<CashFlow>>(`${this.baseUrl}/cash-flow`, { params: this.buildParams({ from, to }) });
  }

  getAging(type: SubLedgerType, asOf?: string): Observable<ApiResponse<SubLedgerEntry[]>> {
    return this.http.get<ApiResponse<SubLedgerEntry[]>>(`${this.baseUrl}/aging`, { params: this.buildParams({ type, asOf }) });
  }

  getDayBook(from: string, to: string): Observable<ApiResponse<DayBookEntry[]>> {
    return this.http.get<ApiResponse<DayBookEntry[]>>(`${this.baseUrl}/day-book`, { params: this.buildParams({ from, to }) });
  }

  getBudgetVsActual(year: number, month: number): Observable<ApiResponse<BudgetVsActualRow[]>> {
    return this.http.get<ApiResponse<BudgetVsActualRow[]>>(`${this.baseUrl}/budget-vs-actual`, {
      params: this.buildParams({ year, month }),
    });
  }

  export(
    type: string,
    format: ExportFormat,
    params: Record<string, string | number | boolean | undefined>,
  ): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${type}/export`, {
      params: this.buildParams({ ...params, format }),
      responseType: 'blob',
    });
  }

  private buildParams(values: Record<string, string | number | boolean | undefined>): Record<string, string> {
    const params: Record<string, string> = {};
    for (const [key, value] of Object.entries(values)) {
      if (value !== undefined && value !== null && value !== '') {
        params[key] = String(value);
      }
    }
    return params;
  }
}
