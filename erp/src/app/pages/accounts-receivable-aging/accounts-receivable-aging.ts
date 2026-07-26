import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { StockLedgerService } from '../../core/stock-ledger/stock-ledger.service';
import { PartnerAging } from '../../core/stock-ledger/stock-ledger.models';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-accounts-receivable-aging',
  imports: [DecimalPipe, CardModule, PrimeTemplate, TableModule],
  templateUrl: './accounts-receivable-aging.html',
  styleUrl: './accounts-receivable-aging.scss',
})
export class AccountsReceivableAging implements OnInit {
  private readonly stockLedgerService = inject(StockLedgerService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly rows = signal<PartnerAging[]>([]);

  ngOnInit(): void {
    this.loading.set(true);
    this.stockLedgerService.getAccountsReceivableAging().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.rows.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load AR aging.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
