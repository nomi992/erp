import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { AccountService } from '../../../core/accounts/account.service';
import { Account } from '../../../core/accounts/account.models';
import { LedgerService } from '../../../core/ledgers/ledger.service';
import { AccountLedger } from '../../../core/ledgers/ledger.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

function toIsoDate(date: Date | null | undefined): string | undefined {
  if (!date) return undefined;
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-account-ledger',
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
  ],
  templateUrl: './account-ledger.html',
  styleUrl: './account-ledger.scss',
})
export class AccountLedgerPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly ledgerService = inject(LedgerService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loadingAccounts = signal(false);
  readonly loading = signal(false);
  readonly accountOptions = signal<{ label: string; value: number }[]>([]);
  readonly ledger = signal<AccountLedger | null>(null);

  form = this.fb.group({
    accountId: this.fb.control<number | null>(null, [Validators.required]),
    dateFrom: this.fb.control<Date | null>(null),
    dateTo: this.fb.control<Date | null>(null),
  });

  ngOnInit(): void {
    this.loadAccounts();

    // Deep link from e.g. the Business Partners "View Ledger" action, which navigates here with
    // ?accountId=<id> instead of making the user re-pick the account from the dropdown.
    const accountIdParam = Number(this.route.snapshot.queryParamMap.get('accountId'));
    if (Number.isInteger(accountIdParam) && accountIdParam > 0) {
      this.form.patchValue({ accountId: accountIdParam });
      this.load();
    }
  }

  loadAccounts(): void {
    this.loadingAccounts.set(true);
    this.accountService.getAll().subscribe({
      next: (response) => {
        this.loadingAccounts.set(false);
        const accounts = response.data ?? [];
        this.accountOptions.set(accounts.map((account: Account) => ({
          label: `${account.code} - ${account.name}`,
          value: account.id,
        })));
      },
      error: (error: HttpErrorResponse) => {
        this.loadingAccounts.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load accounts.'));
      },
    });
  }

  load(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { accountId, dateFrom, dateTo } = this.form.getRawValue();
    if (accountId === null) {
      return;
    }

    this.loading.set(true);
    this.ledgerService.getAccountLedger(accountId, toIsoDate(dateFrom), toIsoDate(dateTo)).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.ledger.set(response.data ?? null);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.ledger.set(null);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load account ledger.'));
      },
    });
  }

  clear(): void {
    this.form.reset({ accountId: null, dateFrom: null, dateTo: null });
    this.ledger.set(null);
  }

  goToVoucher(voucherId: number): void {
    this.router.navigate(['/vouchers', voucherId]);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
