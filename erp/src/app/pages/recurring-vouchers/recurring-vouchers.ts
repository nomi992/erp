import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';
import { AccountService } from '../../core/accounts/account.service';
import { CostCenterService } from '../../core/cost-centers/cost-center.service';
import { RecurringVoucherTemplateService } from '../../core/vouchers/recurring-voucher-template.service';
import {
  RecurringFrequency,
  RecurringTemplateLineRequest,
  RecurringVoucherTemplate,
  RecurringVoucherTemplateRequest,
  VoucherType,
} from '../../core/vouchers/voucher.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

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

const FREQUENCIES: RecurringFrequency[] = ['Daily', 'Weekly', 'Monthly', 'Yearly'];

interface TemplateLineFormControls {
  accountId: FormControl<number | null>;
  debitAmount: FormControl<number>;
  creditAmount: FormControl<number>;
  costCenterId: FormControl<number | null>;
}

type TemplateLineGroup = FormGroup<TemplateLineFormControls>;

interface TemplateLineSeed {
  accountId?: number;
  debitAmount?: number;
  creditAmount?: number;
  costCenterId?: number | null;
}

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
  selector: 'app-recurring-vouchers',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DatePickerModule,
    DialogModule,
    HasRightDirective,
    InputNumberModule,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    ToggleSwitchModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './recurring-vouchers.html',
  styleUrl: './recurring-vouchers.scss',
})
export class RecurringVouchers implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly templateService = inject(RecurringVoucherTemplateService);
  private readonly accountService = inject(AccountService);
  private readonly costCenterService = inject(CostCenterService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly templates = signal<RecurringVoucherTemplate[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingTemplate = signal<RecurringVoucherTemplate | null>(null);

  readonly accountOptions = signal<{ label: string; value: number }[]>([]);
  readonly costCenterOptions = signal<{ label: string; value: number | null }[]>([
    { label: '(None)', value: null },
  ]);

  readonly voucherTypeOptions = VOUCHER_TYPES.map((value) => ({ label: value, value }));
  readonly frequencyOptions = FREQUENCIES.map((value) => ({ label: value, value }));

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    voucherType: this.fb.nonNullable.control<VoucherType>('Journal', [Validators.required]),
    narrationTemplate: [''],
    frequency: this.fb.nonNullable.control<RecurringFrequency>('Monthly', [Validators.required]),
    nextRunDate: this.fb.control<Date | null>(new Date(), [Validators.required]),
    lines: this.fb.array<TemplateLineGroup>([this.createLineGroup(), this.createLineGroup()]),
  });

  get linesArray(): FormArray<TemplateLineGroup> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.templateService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.templates.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load recurring templates.'));
      },
    });
  }

  addLine(): void {
    this.linesArray.push(this.createLineGroup());
  }

  removeLine(index: number): void {
    if (this.linesArray.length > 2) {
      this.linesArray.removeAt(index);
    }
  }

  totalDebit(): number {
    return this.linesArray.getRawValue().reduce((sum, line) => sum + (line.debitAmount ?? 0), 0);
  }

  totalCredit(): number {
    return this.linesArray.getRawValue().reduce((sum, line) => sum + (line.creditAmount ?? 0), 0);
  }

  isBalanced(): boolean {
    return this.totalDebit() > 0 && Math.abs(this.totalDebit() - this.totalCredit()) < 0.005;
  }

  validLineCount(): number {
    return this.linesArray
      .getRawValue()
      .filter((line) => line.accountId && ((line.debitAmount ?? 0) > 0 || (line.creditAmount ?? 0) > 0)).length;
  }

  canSave(): boolean {
    return this.form.valid && this.validLineCount() >= 2 && !this.saving();
  }

  openCreateDialog(): void {
    this.editingTemplate.set(null);
    this.form.reset({
      name: '',
      voucherType: 'Journal',
      narrationTemplate: '',
      frequency: 'Monthly',
      nextRunDate: new Date(),
    });
    this.linesArray.clear();
    this.linesArray.push(this.createLineGroup());
    this.linesArray.push(this.createLineGroup());
    this.dialogVisible.set(true);
  }

  openEditDialog(template: RecurringVoucherTemplate): void {
    this.editingTemplate.set(template);
    this.form.reset({
      name: template.name,
      voucherType: template.voucherType,
      narrationTemplate: template.narrationTemplate,
      frequency: template.frequency,
      nextRunDate: new Date(template.nextRunDate),
    });
    this.linesArray.clear();
    for (const line of template.lines) {
      this.linesArray.push(
        this.createLineGroup({
          accountId: line.accountId,
          debitAmount: line.debitAmount,
          creditAmount: line.creditAmount,
          costCenterId: line.costCenterId,
        }),
      );
    }
    this.dialogVisible.set(true);
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const lines: RecurringTemplateLineRequest[] = this.linesArray
      .getRawValue()
      .filter((line) => line.accountId && ((line.debitAmount ?? 0) > 0 || (line.creditAmount ?? 0) > 0))
      .map((line) => ({
        accountId: line.accountId!,
        debitAmount: line.debitAmount ?? 0,
        creditAmount: line.creditAmount ?? 0,
        costCenterId: line.costCenterId ?? null,
      }));

    const request: RecurringVoucherTemplateRequest = {
      name: raw.name,
      voucherType: raw.voucherType,
      narrationTemplate: raw.narrationTemplate,
      frequency: raw.frequency,
      nextRunDate: toIsoDate(raw.nextRunDate) ?? '',
      lines,
    };

    this.saving.set(true);
    const editing = this.editingTemplate();
    const call = editing ? this.templateService.update(editing.id, request) : this.templateService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Template updated successfully.' : 'Template created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save template.'));
      },
    });
  }

  toggleActive(template: RecurringVoucherTemplate): void {
    const call = template.isActive ? this.templateService.deactivate(template.id) : this.templateService.activate(template.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(template.isActive ? 'Template deactivated.' : 'Template activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update template status.'));
      },
    });
  }

  generateNow(template: RecurringVoucherTemplate): void {
    this.templateService.generateNow(template.id).subscribe({
      next: (response) => {
        this.notificationService.success(response.message);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to generate voucher.'));
      },
    });
  }

  private createLineGroup(line?: TemplateLineSeed): TemplateLineGroup {
    return this.fb.group({
      accountId: this.fb.control<number | null>(line?.accountId ?? null, [Validators.required]),
      debitAmount: this.fb.nonNullable.control<number>(line?.debitAmount ?? 0),
      creditAmount: this.fb.nonNullable.control<number>(line?.creditAmount ?? 0),
      costCenterId: this.fb.control<number | null>(line?.costCenterId ?? null),
    });
  }

  private loadLookups(): void {
    forkJoin({
      accounts: this.accountService.getAll(),
      costCenters: this.costCenterService.getAll(),
    }).subscribe({
      next: ({ accounts, costCenters }) => {
        this.accountOptions.set(
          (accounts.data ?? []).map((account) => ({ label: `${account.code} - ${account.name}`, value: account.id })),
        );
        this.costCenterOptions.set([
          { label: '(None)', value: null },
          ...(costCenters.data ?? []).map((cc) => ({ label: cc.name, value: cc.id as number | null })),
        ]);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load lookup data.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
