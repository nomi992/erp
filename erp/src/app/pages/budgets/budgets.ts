import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { AccountService } from '../../core/accounts/account.service';
import { Account } from '../../core/accounts/account.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { Budget, BudgetRequest } from '../../core/reports/budget.models';
import { BudgetService } from '../../core/reports/budget.service';

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
  selector: 'app-budgets',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    DecimalPipe,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DialogModule,
    HasRightDirective,
    InputNumberModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './budgets.html',
  styleUrl: './budgets.scss',
})
export class Budgets implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly budgetService = inject(BudgetService);
  private readonly accountService = inject(AccountService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly budgets = signal<Budget[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingBudget = signal<Budget | null>(null);
  readonly accountOptions = signal<{ label: string; value: number }[]>([]);

  readonly monthOptions = MONTH_OPTIONS;

  readonly filterYear = signal<number | null>(null);
  readonly filterMonth = signal<number | null>(null);

  form = this.fb.nonNullable.group({
    accountId: this.fb.control<number | null>(null, [Validators.required]),
    year: [new Date().getFullYear(), [Validators.required]],
    month: [new Date().getMonth() + 1, [Validators.required]],
    budgetedAmount: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit(): void {
    this.loadAccounts();
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.budgetService.getAll(this.filterYear() ?? undefined, this.filterMonth() ?? undefined).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.budgets.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load budgets.'));
      },
    });
  }

  loadAccounts(): void {
    this.accountService.getAll().subscribe({
      next: (response) => {
        const options = (response.data ?? []).map((a: Account) => ({ label: `${a.code} - ${a.name}`, value: a.id }));
        this.accountOptions.set(options);
      },
      error: () => {
        // Non-critical: leave the account picker empty.
      },
    });
  }

  openCreateDialog(): void {
    this.editingBudget.set(null);
    this.form.reset({
      accountId: null,
      year: new Date().getFullYear(),
      month: new Date().getMonth() + 1,
      budgetedAmount: 0,
    });
    this.dialogVisible.set(true);
  }

  openEditDialog(budget: Budget): void {
    this.editingBudget.set(budget);
    this.form.reset({
      accountId: budget.accountId,
      year: budget.year,
      month: budget.month,
      budgetedAmount: budget.budgetedAmount,
    });
    this.dialogVisible.set(true);
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();
    const request: BudgetRequest = {
      accountId: raw.accountId as number,
      year: raw.year,
      month: raw.month,
      budgetedAmount: raw.budgetedAmount,
    };
    const editing = this.editingBudget();

    const call = editing ? this.budgetService.update(editing.id, request) : this.budgetService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Budget updated successfully.' : 'Budget created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save budget.'));
      },
    });
  }

  confirmDelete(budget: Budget): void {
    this.confirmationService.confirm({
      header: 'Delete Budget',
      message: `Delete budget for "${budget.accountCode} - ${budget.accountName}" (${budget.year}-${String(budget.month).padStart(2, '0')})? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Delete' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => this.deleteBudget(budget),
    });
  }

  monthName(month: number): string {
    return this.monthOptions.find((m) => m.value === month)?.label ?? String(month);
  }

  private deleteBudget(budget: Budget): void {
    this.budgetService.delete(budget.id).subscribe({
      next: () => {
        this.notificationService.success('Budget deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to delete budget.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
