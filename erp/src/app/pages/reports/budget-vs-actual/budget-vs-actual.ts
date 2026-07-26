import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { PrimeTemplate } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ReportService } from '../../../core/reports/report.service';
import { BudgetVsActualRow } from '../../../core/reports/report.models';

const MONTH_OPTIONS = [
  { label: 'January', value: 1 },
  { label: 'February', value: 2 },
  { label: 'March', value: 3 },
  { label: 'April', value: 4 },
  { label: 'May', value: 5 },
  { label: 'June', value: 6 },
  { label: 'July', value: 7 },
  { label: 'August', value: 8 },
  { label: 'September', value: 9 },
  { label: 'October', value: 10 },
  { label: 'November', value: 11 },
  { label: 'December', value: 12 },
];

@Component({
  selector: 'app-budget-vs-actual-report',
  imports: [
    DecimalPipe,
    FormsModule,
    ButtonModule,
    CardModule,
    InputNumberModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
  ],
  templateUrl: './budget-vs-actual.html',
  styleUrl: './budget-vs-actual.scss',
})
export class BudgetVsActualReport implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingExcel = signal(false);
  readonly rows = signal<BudgetVsActualRow[] | null>(null);
  readonly year = signal<number>(new Date().getFullYear());
  readonly month = signal<number>(new Date().getMonth() + 1);

  readonly monthOptions = MONTH_OPTIONS;

  ngOnInit(): void {
    this.run();
  }

  run(): void {
    this.loading.set(true);
    this.reportService.getBudgetVsActual(this.year(), this.month()).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.rows.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load budget vs actual report.'));
      },
    });
  }

  exportReport(format: 'pdf' | 'excel'): void {
    const busy = format === 'pdf' ? this.exportingPdf : this.exportingExcel;
    busy.set(true);
    this.reportService.export('budget-vs-actual', format, { year: this.year(), month: this.month() }).subscribe({
      next: (blob) => {
        busy.set(false);
        this.downloadBlob(blob, `budget-vs-actual.${format === 'pdf' ? 'pdf' : 'xlsx'}`);
      },
      error: (error: HttpErrorResponse) => {
        busy.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to export budget vs actual report.'));
      },
    });
  }

  varianceSeverity(variance: number): 'success' | 'danger' | 'secondary' {
    if (variance > 0) return 'success';
    if (variance < 0) return 'danger';
    return 'secondary';
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
