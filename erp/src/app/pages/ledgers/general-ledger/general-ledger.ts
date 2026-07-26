import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { LedgerService } from '../../../core/ledgers/ledger.service';
import { GeneralLedgerEntry } from '../../../core/ledgers/ledger.models';
import { VoucherType } from '../../../core/vouchers/voucher.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

const VOUCHER_TYPES: VoucherType[] = [
  'Payment',
  'Receipt',
  'Journal',
  'Sales',
  'Purchase',
  'Contra',
  'DebitNote',
  'CreditNote',
];

function toIsoDate(date: Date | null | undefined): string | undefined {
  if (!date) return undefined;
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-general-ledger',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    PrimeTemplate,
    DatePickerModule,
    SelectModule,
    TableModule,
    TagModule,
  ],
  templateUrl: './general-ledger.html',
  styleUrl: './general-ledger.scss',
})
export class GeneralLedger implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly ledgerService = inject(LedgerService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly entries = signal<GeneralLedgerEntry[]>([]);

  readonly voucherTypeOptions = VOUCHER_TYPES.map((value) => ({ label: value, value }));

  form = this.fb.group({
    voucherType: this.fb.control<VoucherType | null>(null),
    dateFrom: this.fb.control<Date | null>(null),
    dateTo: this.fb.control<Date | null>(null),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const { voucherType, dateFrom, dateTo } = this.form.getRawValue();
    this.loading.set(true);
    this.ledgerService
      .getGeneralLedger(toIsoDate(dateFrom), toIsoDate(dateTo), voucherType ?? undefined)
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          this.entries.set(response.data ?? []);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          this.notificationService.error(this.extractErrorMessage(error, 'Unable to load general ledger.'));
        },
      });
  }

  clear(): void {
    this.form.reset({ voucherType: null, dateFrom: null, dateTo: null });
    this.load();
  }

  goToVoucher(voucherId: number): void {
    this.router.navigate(['/vouchers', voucherId]);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
