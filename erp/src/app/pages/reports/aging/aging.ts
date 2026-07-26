import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { PrimeTemplate } from 'primeng/api';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ApiResponse } from '../../../core/models/api-response.model';
import { SubLedgerEntry, SubLedgerType } from '../../../core/ledgers/ledger.models';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ReportService } from '../../../core/reports/report.service';

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-aging-report',
  imports: [
    DecimalPipe,
    FormsModule,
    ButtonModule,
    CardModule,
    DatePickerModule,
    PrimeTemplate,
    SelectButtonModule,
    TableModule,
    TagModule,
  ],
  templateUrl: './aging.html',
  styleUrl: './aging.scss',
})
export class AgingReport implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingExcel = signal(false);
  readonly entries = signal<SubLedgerEntry[] | null>(null);
  readonly asOf = signal<Date>(new Date());
  readonly subLedgerType = signal<SubLedgerType>('Receivable');

  readonly typeOptions: { label: string; value: SubLedgerType }[] = [
    { label: 'Receivable', value: 'Receivable' },
    { label: 'Payable', value: 'Payable' },
  ];

  ngOnInit(): void {
    this.run();
  }

  run(): void {
    this.loading.set(true);
    const asOf = toIsoDate(this.asOf());
    this.reportService.getAging(this.subLedgerType(), asOf).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.entries.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.entries.set(null);
        this.notificationService.error(
          this.extractErrorMessage(
            error,
            `Unable to load ${this.subLedgerType().toLowerCase()} aging report. Is a control account configured for this type?`,
          ),
        );
      },
    });
  }

  exportReport(format: 'pdf' | 'excel'): void {
    const busy = format === 'pdf' ? this.exportingPdf : this.exportingExcel;
    busy.set(true);
    const asOf = toIsoDate(this.asOf());
    this.reportService.export('aging', format, { subLedgerType: this.subLedgerType(), asOf }).subscribe({
      next: (blob) => {
        busy.set(false);
        this.downloadBlob(blob, `aging-${this.subLedgerType().toLowerCase()}.${format === 'pdf' ? 'pdf' : 'xlsx'}`);
      },
      error: (error: HttpErrorResponse) => {
        busy.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to export aging report.'));
      },
    });
  }

  ageBucketSeverity(ageInDays: number): 'success' | 'info' | 'warn' | 'danger' {
    if (ageInDays <= 30) return 'success';
    if (ageInDays <= 60) return 'info';
    if (ageInDays <= 90) return 'warn';
    return 'danger';
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
