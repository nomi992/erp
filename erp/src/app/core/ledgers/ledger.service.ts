import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { VoucherType } from '../vouchers/voucher.models';
import { AccountLedger, BankStatementLine, GeneralLedgerEntry, SubLedgerEntry, SubLedgerType } from './ledger.models';

@Injectable({ providedIn: 'root' })
export class LedgerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/ledgers`;

  getGeneralLedger(from?: string, to?: string, voucherType?: VoucherType): Observable<ApiResponse<GeneralLedgerEntry[]>> {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    if (voucherType) params['voucherType'] = voucherType;

    return this.http.get<ApiResponse<GeneralLedgerEntry[]>>(`${this.baseUrl}/general`, { params });
  }

  getAccountLedger(accountId: number, from?: string, to?: string): Observable<ApiResponse<AccountLedger>> {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;

    return this.http.get<ApiResponse<AccountLedger>>(`${this.baseUrl}/account/${accountId}`, { params });
  }

  getSubLedger(type: SubLedgerType, asOf?: string): Observable<ApiResponse<SubLedgerEntry[]>> {
    const params: Record<string, string> = { type };
    if (asOf) params['asOf'] = asOf;

    return this.http.get<ApiResponse<SubLedgerEntry[]>>(`${this.baseUrl}/subledger`, { params });
  }

  getBankLines(accountId: number, matched?: boolean): Observable<ApiResponse<BankStatementLine[]>> {
    const params: Record<string, string> = {};
    if (matched !== undefined) params['matched'] = String(matched);

    return this.http.get<ApiResponse<BankStatementLine[]>>(`${this.baseUrl}/bank/${accountId}/lines`, { params });
  }

  importBankStatement(accountId: number, file: File): Observable<ApiResponse<{ imported: number }>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<{ imported: number }>>(`${this.baseUrl}/bank/${accountId}/import`, formData);
  }

  autoMatch(accountId: number): Observable<ApiResponse<{ matchedCount: number; remaining: number }>> {
    return this.http.post<ApiResponse<{ matchedCount: number; remaining: number }>>(`${this.baseUrl}/bank/${accountId}/auto-match`, {});
  }

  manualMatch(bankStatementLineId: number, voucherDetailId: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/bank/match`, { bankStatementLineId, voucherDetailId });
  }

  unmatch(bankStatementLineId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/bank/match/${bankStatementLineId}`);
  }
}
