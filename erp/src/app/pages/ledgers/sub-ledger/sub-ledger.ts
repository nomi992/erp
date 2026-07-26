import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DatePickerModule } from 'primeng/datepicker';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { LedgerService } from '../../../core/ledgers/ledger.service';
import { SubLedgerEntry, SubLedgerType } from '../../../core/ledgers/ledger.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

function toIsoDate(date: Date | null | undefined): string | undefined {
  if (!date) return undefined;
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function ageBucketSeverity(bucket: string): 'success' | 'info' | 'warn' | 'danger' {
  const normalized = bucket.replace(/\s+/g, '');
  if (normalized.includes('0-30')) return 'success';
  if (normalized.includes('31-60')) return 'info';
  if (normalized.includes('61-90')) return 'warn';
  return 'danger';
}

@Component({
  selector: 'app-sub-ledger',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    PrimeTemplate,
    DatePickerModule,
    TableModule,
    TagModule,
  ],
  templateUrl: './sub-ledger.html',
  styleUrl: './sub-ledger.scss',
})
export class SubLedger implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly ledgerService = inject(LedgerService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly type = signal<SubLedgerType>('Receivable');
  readonly entries = signal<SubLedgerEntry[]>([]);

  form = this.fb.group({
    asOf: this.fb.control<Date | null>(new Date()),
  });

  ngOnInit(): void {
    this.load();
  }

  selectType(type: SubLedgerType): void {
    if (this.type() === type) return;
    this.type.set(type);
    this.load();
  }

  load(): void {
    const { asOf } = this.form.getRawValue();
    this.loading.set(true);
    this.ledgerService.getSubLedger(this.type(), toIsoDate(asOf)).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.entries.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.entries.set([]);
        this.notificationService.error(this.extractErrorMessage(error, `Unable to load ${this.type().toLowerCase()} sub-ledger.`));
      },
    });
  }

  severityFor(bucket: string): 'success' | 'info' | 'warn' | 'danger' {
    return ageBucketSeverity(bucket);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
