import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { VoucherFilter, VoucherService } from '../../../core/vouchers/voucher.service';
import { VoucherListItem, VoucherStatus, VoucherType } from '../../../core/vouchers/voucher.models';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
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

const VOUCHER_STATUSES: VoucherStatus[] = ['Draft', 'PendingApproval', 'Posted', 'Rejected'];

function toIsoDate(date: Date | null): string | undefined {
  if (!date) {
    return undefined;
  }
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-voucher-list',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DatePickerModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './voucher-list.html',
  styleUrl: './voucher-list.scss',
})
export class VoucherList implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly voucherService = inject(VoucherService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly vouchers = signal<VoucherListItem[]>([]);

  readonly typeOptions = [
    { label: 'All Types', value: null },
    ...VOUCHER_TYPES.map((value) => ({ label: value, value })),
  ];

  readonly statusOptions = [
    { label: 'All Statuses', value: null },
    ...VOUCHER_STATUSES.map((value) => ({ label: value, value })),
  ];

  filterForm = this.fb.group({
    type: this.fb.control<VoucherType | null>(null),
    status: this.fb.control<VoucherStatus | null>(null),
    from: this.fb.control<Date | null>(null),
    to: this.fb.control<Date | null>(null),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const raw = this.filterForm.getRawValue();
    const filter: VoucherFilter = {
      type: raw.type ?? undefined,
      status: raw.status ?? undefined,
      from: toIsoDate(raw.from),
      to: toIsoDate(raw.to),
    };

    this.loading.set(true);
    this.voucherService.getAll(filter).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.vouchers.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load vouchers.'));
      },
    });
  }

  clearFilters(): void {
    this.filterForm.reset({ type: null, status: null, from: null, to: null });
    this.load();
  }

  statusSeverity(status: VoucherStatus): 'secondary' | 'warn' | 'success' | 'danger' {
    switch (status) {
      case 'Draft':
        return 'secondary';
      case 'PendingApproval':
        return 'warn';
      case 'Posted':
        return 'success';
      case 'Rejected':
        return 'danger';
    }
  }

  viewVoucher(voucher: VoucherListItem): void {
    this.router.navigate(['/vouchers', voucher.id]);
  }

  createVoucher(): void {
    this.router.navigate(['/vouchers/new']);
  }

  createPaymentVoucher(): void {
    this.router.navigate(['/vouchers/payment/new']);
  }

  createReceiptVoucher(): void {
    this.router.navigate(['/vouchers/receipt/new']);
  }

  editVoucher(voucher: VoucherListItem): void {
    this.router.navigate(['/vouchers', voucher.id, 'edit']);
  }

  submitVoucher(voucher: VoucherListItem): void {
    this.voucherService.submit(voucher.id).subscribe({
      next: () => {
        this.notificationService.success(`Voucher ${voucher.voucherNo} submitted for approval.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit voucher.'));
      },
    });
  }

  approveVoucher(voucher: VoucherListItem): void {
    this.voucherService.approve(voucher.id).subscribe({
      next: () => {
        this.notificationService.success(`Voucher ${voucher.voucherNo} approved and posted.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve voucher.'));
      },
    });
  }

  rejectVoucher(voucher: VoucherListItem): void {
    this.voucherService.reject(voucher.id).subscribe({
      next: () => {
        this.notificationService.success(`Voucher ${voucher.voucherNo} rejected.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject voucher.'));
      },
    });
  }

  confirmReverse(voucher: VoucherListItem): void {
    this.confirmationService.confirm({
      header: 'Reverse Voucher',
      message: `Reverse posted voucher "${voucher.voucherNo}"? This will create a new voucher with debit/credit amounts swapped.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Reverse' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => this.reverseVoucher(voucher),
    });
  }

  private reverseVoucher(voucher: VoucherListItem): void {
    this.voucherService.reverse(voucher.id).subscribe({
      next: (response) => {
        const reversal = response.data;
        this.notificationService.success(
          reversal ? `Reversal voucher ${reversal.voucherNo} created.` : 'Voucher reversed successfully.',
        );
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reverse voucher.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
