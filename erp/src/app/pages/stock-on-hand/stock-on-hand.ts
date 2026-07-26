import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { PrimeTemplate } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { StockLedgerService } from '../../core/stock-ledger/stock-ledger.service';
import { StockOnHand } from '../../core/stock-ledger/stock-ledger.models';
import { WarehouseService } from '../../core/warehouses/warehouse.service';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-stock-on-hand',
  imports: [ReactiveFormsModule, DecimalPipe, CardModule, CheckboxModule, PrimeTemplate, SelectModule, TableModule, TagModule],
  templateUrl: './stock-on-hand.html',
  styleUrl: './stock-on-hand.scss',
})
export class StockOnHandPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly stockLedgerService = inject(StockLedgerService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly items = signal<StockOnHand[]>([]);
  readonly warehouseOptions = signal<{ label: string; value: number | null }[]>([{ label: 'All Warehouses', value: null }]);

  filterForm = this.fb.group({
    warehouseId: this.fb.control<number | null>(null),
    lowStockOnly: this.fb.nonNullable.control<boolean>(false),
  });

  ngOnInit(): void {
    this.warehouseService.getAll().subscribe({
      next: (response) => {
        this.warehouseOptions.set([
          { label: 'All Warehouses', value: null },
          ...(response.data ?? []).map((w) => ({ label: w.name, value: w.id as number | null })),
        ]);
      },
    });
    this.load();
  }

  load(): void {
    const raw = this.filterForm.getRawValue();
    this.loading.set(true);
    this.stockLedgerService.getOnHand({ warehouseId: raw.warehouseId ?? undefined, lowStockOnly: raw.lowStockOnly }).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.items.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load stock on hand.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
