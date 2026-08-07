import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate, TreeNode } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';
import { TreeModule } from 'primeng/tree';
import { AccountService } from '../../core/accounts/account.service';
import {
  Account,
  AccountNature,
  AccountTreeNode,
  AccountType,
  CreateAccountRequest,
} from '../../core/accounts/account.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

const ACCOUNT_TYPES: AccountType[] = ['Asset', 'Liability', 'Equity', 'Income', 'Expense'];
const ACCOUNT_NATURES: AccountNature[] = ['Debit', 'Credit'];

function mapToTreeNode(node: AccountTreeNode): TreeNode<AccountTreeNode> {
  return {
    key: String(node.id),
    data: node,
    expanded: true,
    children: node.children.map(mapToTreeNode),
  };
}

function collectDescendantIds(node: AccountTreeNode, ids: Set<number>): void {
  for (const child of node.children) {
    ids.add(child.id);
    collectDescendantIds(child, ids);
  }
}

function flatten(nodes: AccountTreeNode[], acc: AccountTreeNode[] = []): AccountTreeNode[] {
  for (const node of nodes) {
    acc.push(node);
    flatten(node.children, acc);
  }
  return acc;
}

@Component({
  selector: 'app-chart-of-accounts',
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DialogModule,
    HasRightDirective,
    InputNumberModule,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TagModule,
    ToggleSwitchModule,
    TooltipModule,
    TreeModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './chart-of-accounts.html',
  styleUrl: './chart-of-accounts.scss',
})
export class ChartOfAccounts implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly treeNodes = signal<TreeNode<AccountTreeNode>[]>([]);
  readonly flatAccounts = signal<AccountTreeNode[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingAccount = signal<AccountTreeNode | null>(null);

  readonly typeOptions = ACCOUNT_TYPES.map((value) => ({ label: value, value }));
  readonly natureOptions = ACCOUNT_NATURES.map((value) => ({ label: value, value }));
  readonly parentOptions = signal<{ label: string; value: number | null }[]>([]);

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required]],
    name: ['', [Validators.required]],
    type: this.fb.nonNullable.control<AccountType>('Asset', [Validators.required]),
    nature: this.fb.nonNullable.control<AccountNature>('Debit', [Validators.required]),
    parentAccountId: this.fb.control<number | null>(null),
    isControlAccount: [false],
    isCashAccount: [false],
    isBankAccount: [false],
    openingBalance: [0],
    openingBalanceNature: this.fb.nonNullable.control<AccountNature>('Debit', [Validators.required]),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.accountService.getTree().subscribe({
      next: (response) => {
        this.loading.set(false);
        const nodes = response.data ?? [];
        this.treeNodes.set(nodes.map(mapToTreeNode));
        this.flatAccounts.set(flatten(nodes));
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load chart of accounts.'));
      },
    });
  }

  openCreateDialog(parent: AccountTreeNode | null): void {
    this.editingAccount.set(null);
    this.form.reset({
      code: '',
      name: '',
      type: 'Asset',
      nature: 'Debit',
      parentAccountId: parent?.id ?? null,
      isControlAccount: false,
      isCashAccount: false,
      isBankAccount: false,
      openingBalance: 0,
      openingBalanceNature: 'Debit',
    });
    this.setParentOptions(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(account: AccountTreeNode): void {
    this.editingAccount.set(account);
    this.form.reset({
      code: account.code,
      name: account.name,
      type: account.type,
      nature: account.nature,
      parentAccountId: account.parentAccountId,
      isControlAccount: account.isControlAccount,
      isCashAccount: account.isCashAccount,
      isBankAccount: account.isBankAccount,
      openingBalance: account.openingBalance,
      openingBalanceNature: account.openingBalanceNature,
    });
    this.setParentOptions(account);
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
    const request: CreateAccountRequest = this.form.getRawValue();
    const editing = this.editingAccount();

    const call = editing
      ? this.accountService.update(editing.id, request)
      : this.accountService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Account updated successfully.' : 'Account created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save account.'));
      },
    });
  }

  toggleActive(account: AccountTreeNode): void {
    const call = account.isActive
      ? this.accountService.deactivate(account.id)
      : this.accountService.activate(account.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(account.isActive ? 'Account deactivated.' : 'Account activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update account status.'));
      },
    });
  }

  confirmDelete(account: AccountTreeNode): void {
    this.confirmationService.confirm({
      header: 'Delete Account',
      message: `Delete account "${account.code} - ${account.name}"? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Delete' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => this.deleteAccount(account),
    });
  }

  private deleteAccount(account: AccountTreeNode): void {
    this.accountService.delete(account.id).subscribe({
      next: () => {
        this.notificationService.success('Account deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to delete account.'));
      },
    });
  }

  private setParentOptions(editing: AccountTreeNode | null): void {
    const excluded = new Set<number>();
    if (editing) {
      excluded.add(editing.id);
      collectDescendantIds(editing, excluded);
    }

    const options = this.flatAccounts()
      .filter((a) => !excluded.has(a.id))
      .map((a) => ({ label: `${a.code} - ${a.name}`, value: a.id }));

    this.parentOptions.set([{ label: '(None — root account)', value: null }, ...options]);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
