import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { AccountService } from '../../core/accounts/account.service';
import { Account } from '../../core/accounts/account.models';
import { VoucherService } from '../../core/vouchers/voucher.service';
import { CreateVoucherRequest, VoucherLineRequest } from '../../core/vouchers/voucher.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { SimpleVoucherConfig } from './simple-voucher-config';

function toIsoDate(date: Date | null): string {
  if (!date) return '';
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Simplified Payment/Receipt Voucher entry: the user picks one posting account
 * (the expense/income/party side), flags whether the other side is a bank
 * account or plain cash, enters an amount and narration — no debit/credit
 * columns. The two balanced `VoucherDetail` lines are built here and sent to
 * the same `POST /api/vouchers` endpoint the generic voucher form uses, so it
 * lands as a normal Draft voucher subject to the existing approval workflow.
 */
@Component({
  selector: 'app-simple-voucher-form',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DatePickerModule,
    HasRightDirective,
    InputNumberModule,
    SelectModule,
    TextareaModule,
    ToggleSwitchModule,
  ],
  templateUrl: './simple-voucher-form.html',
  styleUrl: './simple-voucher-form.scss',
})
export class SimpleVoucherForm implements OnInit {
  readonly config = input.required<SimpleVoucherConfig>();

  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);
  private readonly voucherService = inject(VoucherService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly accounts = signal<Account[]>([]);

  form = this.fb.nonNullable.group({
    date: this.fb.control<Date | null>(new Date(), [Validators.required]),
    postingAccountId: this.fb.control<number | null>(null, [Validators.required]),
    isBank: this.fb.nonNullable.control<boolean>(false),
    bankOrCashAccountId: this.fb.control<number | null>(null, [Validators.required]),
    amount: this.fb.nonNullable.control<number>(0, [Validators.required, Validators.min(0.01)]),
    narration: this.fb.control<string | null>(null),
  });

  // Posting account = the "other side" of the entry: any account that isn't
  // itself a bank/cash account, so the same account can't be picked for both sides.
  readonly postingAccountOptions = computed(() =>
    this.accounts()
      .filter((a) => !a.isCashAccount && !a.isBankAccount)
      .map((a) => ({ label: `${a.code} - ${a.name}`, value: a.id })),
  );

  readonly bankAccountOptions = computed(() =>
    this.accounts()
      .filter((a) => a.isBankAccount)
      .map((a) => ({ label: `${a.code} - ${a.name}`, value: a.id })),
  );

  readonly cashAccountOptions = computed(() =>
    this.accounts()
      .filter((a) => a.isCashAccount)
      .map((a) => ({ label: `${a.code} - ${a.name}`, value: a.id })),
  );

  ngOnInit(): void {
    this.loading.set(true);
    this.accountService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.accounts.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load accounts.'));
      },
    });
  }

  bankOrCashOptions(): { label: string; value: number }[] {
    return this.form.controls.isBank.value ? this.bankAccountOptions() : this.cashAccountOptions();
  }

  onBankToggleChange(): void {
    // The other side's account list just changed (bank vs. cash) — clear the stale pick.
    this.form.controls.bankOrCashAccountId.setValue(null);
  }

  canSave(): boolean {
    return !this.saving() && this.form.valid;
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const postingAccountId = raw.postingAccountId!;
    const bankOrCashAccountId = raw.bankOrCashAccountId!;
    const amount = raw.amount;

    // Payment: Dr posting account (expense/payable/etc.) / Cr bank or cash.
    // Receipt: Dr bank or cash / Cr posting account (income/receivable/etc.).
    const lines: VoucherLineRequest[] =
      this.config().voucherType === 'Payment'
        ? [
            { accountId: postingAccountId, debitAmount: amount, creditAmount: 0, costCenterId: null, taxRateId: null },
            { accountId: bankOrCashAccountId, debitAmount: 0, creditAmount: amount, costCenterId: null, taxRateId: null },
          ]
        : [
            { accountId: bankOrCashAccountId, debitAmount: amount, creditAmount: 0, costCenterId: null, taxRateId: null },
            { accountId: postingAccountId, debitAmount: 0, creditAmount: amount, costCenterId: null, taxRateId: null },
          ];

    const request: CreateVoucherRequest = {
      voucherType: this.config().voucherType,
      date: toIsoDate(raw.date),
      narration: raw.narration ?? '',
      currencyCode: 'USD',
      exchangeRate: 1,
      lines,
    };

    this.saving.set(true);
    this.voucherService.create(request).subscribe({
      next: (response) => {
        this.saving.set(false);
        const voucherNo = response.data?.voucherNo;
        this.notificationService.success(voucherNo ? `${voucherNo} created as Draft.` : 'Voucher created as Draft.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save voucher.'));
      },
    });
  }

  cancel(): void {
    this.router.navigate([this.config().listRoute]);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
