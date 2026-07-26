import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageModule } from 'primeng/message';
import { PrimeTemplate } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { CostCenterService } from '../../../core/cost-centers/cost-center.service';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ReportService } from '../../../core/reports/report.service';
import { TrialBalance } from '../../../core/reports/report.models';

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-trial-balance-report',
  imports: [
    DecimalPipe,
    FormsModule,
    ButtonModule,
    CardModule,
    DatePickerModule,
    MessageModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
  ],
  templateUrl: './trial-balance.html',
  styleUrl: './trial-balance.scss',
})
export class TrialBalanceReport implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly costCenterService = inject(CostCenterService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingExcel = signal(false);
  readonly report = signal<TrialBalance | null>(null);
  readonly asOf = signal<Date>(new Date());
  readonly costCenterId = signal<number | null>(null);
  readonly costCenterOptions = signal<{ label: string; value: number | null }[]>([
    { label: 'All Cost Centers', value: null },
  ]);

  ngOnInit(): void {
    this.loadCostCenters();
    this.run();
  }

  loadCostCenters(): void {
    this.costCenterService.getAll().subscribe({
      next: (response) => {
        const options = (response.data ?? []).map((c) => ({ label: c.name, value: c.id as number | null }));
        this.costCenterOptions.set([{ label: 'All Cost Centers', value: null }, ...options]);
      },
      error: () => {
        // Non-critical: leave the filter with just the default option.
      },
    });
  }

  run(): void {
    this.loading.set(true);
    const asOf = toIsoDate(this.asOf());
    const costCenterId = this.costCenterId() ?? undefined;
    this.reportService.getTrialBalance(asOf, costCenterId).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.report.set(response.data ?? null);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load trial balance.'));
      },
    });
  }

  exportReport(format: 'pdf' | 'excel'): void {
    const busy = format === 'pdf' ? this.exportingPdf : this.exportingExcel;
    busy.set(true);
    const asOf = toIsoDate(this.asOf());
    const costCenterId = this.costCenterId() ?? undefined;
    this.reportService.export('trial-balance', format, { asOf, costCenterId }).subscribe({
      next: (blob) => {
        busy.set(false);
        this.downloadBlob(blob, `trial-balance.${format === 'pdf' ? 'pdf' : 'xlsx'}`);
      },
      error: (error: HttpErrorResponse) => {
        busy.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to export trial balance.'));
      },
    });
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
