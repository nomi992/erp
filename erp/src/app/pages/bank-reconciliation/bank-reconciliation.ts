import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { FileUpload, FileUploadHandlerEvent, FileUploadModule } from 'primeng/fileupload';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { AccountService } from '../../core/accounts/account.service';
import { Account } from '../../core/accounts/account.models';
import { LedgerService } from '../../core/ledgers/ledger.service';
import { BankStatementLine } from '../../core/ledgers/ledger.models';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-bank-reconciliation',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    PrimeTemplate,
    DialogModule,
    FileUploadModule,
    InputNumberModule,
    SelectModule,
    TableModule,
  ],
  templateUrl: './bank-reconciliation.html',
  styleUrl: './bank-reconciliation.scss',
})
export class BankReconciliation implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly ledgerService = inject(LedgerService);
  private readonly notificationService = inject(NotificationService);

  readonly loadingAccounts = signal(false);
  readonly loadingLines = signal(false);
  readonly importing = signal(false);
  readonly autoMatching = signal(false);
  readonly matching = signal(false);

  readonly accountOptions = signal<{ label: string; value: number }[]>([]);
  readonly selectedAccountId = signal<number | null>(null);
  readonly lines = signal<BankStatementLine[]>([]);

  readonly unmatchedLines = computed(() => this.lines().filter((line) => !line.isMatched));
  readonly matchedLines = computed(() => this.lines().filter((line) => line.isMatched));

  readonly matchDialogVisible = signal(false);
  readonly matchingLine = signal<BankStatementLine | null>(null);

  readonly accountControl = this.fb.control<number | null>(null);

  matchForm = this.fb.nonNullable.group({
    voucherDetailId: this.fb.nonNullable.control<number | null>(null, [Validators.required]),
  });

  ngOnInit(): void {
    this.loadAccounts();
    this.accountControl.valueChanges.subscribe((accountId) => this.onAccountChange(accountId));
  }

  loadAccounts(): void {
    this.loadingAccounts.set(true);
    this.accountService.getAll().subscribe({
      next: (response) => {
        this.loadingAccounts.set(false);
        const accounts = (response.data ?? []).filter((account: Account) => account.isCashAccount);
        this.accountOptions.set(accounts.map((account) => ({ label: `${account.code} - ${account.name}`, value: account.id })));
      },
      error: (error: HttpErrorResponse) => {
        this.loadingAccounts.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load cash/bank accounts.'));
      },
    });
  }

  onAccountChange(accountId: number | null): void {
    this.selectedAccountId.set(accountId);
    this.lines.set([]);
    if (accountId !== null) {
      this.loadLines(accountId);
    }
  }

  loadLines(accountId: number): void {
    this.loadingLines.set(true);
    this.ledgerService.getBankLines(accountId).subscribe({
      next: (response) => {
        this.loadingLines.set(false);
        this.lines.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loadingLines.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load bank statement lines.'));
      },
    });
  }

  onImport(event: FileUploadHandlerEvent, fileUpload: FileUpload): void {
    const accountId = this.selectedAccountId();
    const file = event.files[0];
    if (accountId === null || !file) {
      return;
    }

    this.importing.set(true);
    this.ledgerService.importBankStatement(accountId, file).subscribe({
      next: (response) => {
        this.importing.set(false);
        fileUpload.clear();
        const imported = response.data?.imported ?? 0;
        this.notificationService.success(`Imported ${imported} bank statement line(s).`);
        this.loadLines(accountId);
      },
      error: (error: HttpErrorResponse) => {
        this.importing.set(false);
        fileUpload.clear();
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to import bank statement.'));
      },
    });
  }

  autoMatch(): void {
    const accountId = this.selectedAccountId();
    if (accountId === null) return;

    this.autoMatching.set(true);
    this.ledgerService.autoMatch(accountId).subscribe({
      next: (response) => {
        this.autoMatching.set(false);
        const matchedCount = response.data?.matchedCount ?? 0;
        const remaining = response.data?.remaining ?? 0;
        this.notificationService.success(`Auto-match complete: ${matchedCount} matched, ${remaining} remaining.`);
        this.loadLines(accountId);
      },
      error: (error: HttpErrorResponse) => {
        this.autoMatching.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to auto-match bank statement lines.'));
      },
    });
  }

  openMatchDialog(line: BankStatementLine): void {
    this.matchingLine.set(line);
    this.matchForm.reset({ voucherDetailId: null });
    this.matchDialogVisible.set(true);
  }

  closeMatchDialog(): void {
    this.matchDialogVisible.set(false);
    this.matchingLine.set(null);
  }

  confirmMatch(): void {
    if (this.matchForm.invalid) {
      this.matchForm.markAllAsTouched();
      return;
    }

    const line = this.matchingLine();
    const accountId = this.selectedAccountId();
    const { voucherDetailId } = this.matchForm.getRawValue();
    if (!line || accountId === null || voucherDetailId === null) {
      return;
    }

    this.matching.set(true);
    this.ledgerService.manualMatch(line.id, voucherDetailId).subscribe({
      next: () => {
        this.matching.set(false);
        this.notificationService.success('Bank statement line matched successfully.');
        this.matchDialogVisible.set(false);
        this.matchingLine.set(null);
        this.loadLines(accountId);
      },
      error: (error: HttpErrorResponse) => {
        this.matching.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to match bank statement line.'));
      },
    });
  }

  unmatch(line: BankStatementLine): void {
    const accountId = this.selectedAccountId();
    if (accountId === null) return;

    this.ledgerService.unmatch(line.id).subscribe({
      next: () => {
        this.notificationService.success('Bank statement line unmatched.');
        this.loadLines(accountId);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to unmatch bank statement line.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
