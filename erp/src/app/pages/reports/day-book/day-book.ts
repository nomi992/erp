import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ReportService } from '../../../core/reports/report.service';
import { DayBookEntry } from '../../../core/reports/report.models';

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-day-book-report',
  imports: [DecimalPipe, FormsModule, ButtonModule, CardModule, DatePickerModule, PrimeTemplate, TableModule, TagModule],
  templateUrl: './day-book.html',
  styleUrl: './day-book.scss',
})
export class DayBookReport implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingExcel = signal(false);
  readonly entries = signal<DayBookEntry[] | null>(null);
  readonly from = signal<Date>(new Date());
  readonly to = signal<Date>(new Date());

  ngOnInit(): void {
    this.run();
  }

  run(): void {
    this.loading.set(true);
    const from = toIsoDate(this.from());
    const to = toIsoDate(this.to());
    this.reportService.getDayBook(from, to).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.entries.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load day book.'));
      },
    });
  }

  exportReport(format: 'pdf' | 'excel'): void {
    const busy = format === 'pdf' ? this.exportingPdf : this.exportingExcel;
    busy.set(true);
    const from = toIsoDate(this.from());
    const to = toIsoDate(this.to());
    this.reportService.export('day-book', format, { from, to }).subscribe({
      next: (blob) => {
        busy.set(false);
        this.downloadBlob(blob, `day-book.${format === 'pdf' ? 'pdf' : 'xlsx'}`);
      },
      error: (error: HttpErrorResponse) => {
        busy.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to export day book.'));
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
