import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ReportService } from '../../../core/reports/report.service';
import { CashFlow, CashFlowLine } from '../../../core/reports/report.models';

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

@Component({
  selector: 'app-cash-flow-report',
  imports: [DecimalPipe, FormsModule, ButtonModule, CardModule, DatePickerModule, PrimeTemplate, TableModule],
  templateUrl: './cash-flow.html',
  styleUrl: './cash-flow.scss',
})
export class CashFlowReport implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingExcel = signal(false);
  readonly report = signal<CashFlow | null>(null);
  readonly from = signal<Date>(startOfMonth(new Date()));
  readonly to = signal<Date>(new Date());

  readonly operatingLines = computed<CashFlowLine[]>(() => this.linesFor('Operating'));
  readonly investingLines = computed<CashFlowLine[]>(() => this.linesFor('Investing'));
  readonly financingLines = computed<CashFlowLine[]>(() => this.linesFor('Financing'));

  ngOnInit(): void {
    this.run();
  }

  run(): void {
    this.loading.set(true);
    const from = toIsoDate(this.from());
    const to = toIsoDate(this.to());
    this.reportService.getCashFlow(from, to).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.report.set(response.data ?? null);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load cash flow report.'));
      },
    });
  }

  exportReport(format: 'pdf' | 'excel'): void {
    const busy = format === 'pdf' ? this.exportingPdf : this.exportingExcel;
    busy.set(true);
    const from = toIsoDate(this.from());
    const to = toIsoDate(this.to());
    this.reportService.export('cash-flow', format, { from, to }).subscribe({
      next: (blob) => {
        busy.set(false);
        this.downloadBlob(blob, `cash-flow.${format === 'pdf' ? 'pdf' : 'xlsx'}`);
      },
      error: (error: HttpErrorResponse) => {
        busy.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to export cash flow report.'));
      },
    });
  }

  private linesFor(category: CashFlowLine['category']): CashFlowLine[] {
    return this.report()?.lines.filter((l) => l.category === category) ?? [];
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
