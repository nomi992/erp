import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { TableLazyLoadEvent } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { StockLedgerService } from '../../core/stock-ledger/stock-ledger.service';
import { StockLedgerEntry } from '../../core/stock-ledger/stock-ledger.models';
import { WarehouseService } from '../../core/warehouses/warehouse.service';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-stock-ledger',
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe, CardModule, PrimeTemplate, SelectModule, TableModule],
  templateUrl: './stock-ledger.html',
  styleUrl: './stock-ledger.scss',
})
export class StockLedgerPage {
  private readonly fb = inject(FormBuilder);
  private readonly stockLedgerService = inject(StockLedgerService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly entries = signal<StockLedgerEntry[]>([]);
  readonly totalRecords = signal(0);
  readonly rows = signal(25);
  readonly warehouseOptions = signal<{ label: string; value: number | null }[]>([{ label: 'All Warehouses', value: null }]);

  filterForm = this.fb.group({
    warehouseId: this.fb.control<number | null>(null),
  });

  constructor() {
    this.warehouseService.getAll().subscribe({
      next: (response) => {
        this.warehouseOptions.set([
          { label: 'All Warehouses', value: null },
          ...(response.data ?? []).map((w) => ({ label: w.name, value: w.id as number | null })),
        ]);
      },
    });
  }

  load(event?: TableLazyLoadEvent): void {
    const first = event?.first ?? 0;
    const rows = event?.rows ?? this.rows();
    const pageNumber = Math.floor(first / rows) + 1;
    const warehouseId = this.filterForm.getRawValue().warehouseId ?? undefined;

    this.loading.set(true);
    this.stockLedgerService.getLedger({ warehouseId, pageNumber, pageSize: rows }).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.entries.set(response.data?.items ?? []);
        this.totalRecords.set(response.data?.totalCount ?? 0);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load the stock ledger.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
