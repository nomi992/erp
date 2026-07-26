import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { FileUpload, FileUploadHandlerEvent, FileUploadModule } from 'primeng/fileupload';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { AccountService } from '../../../core/accounts/account.service';
import { CostCenterService } from '../../../core/cost-centers/cost-center.service';
import { TaxRateService } from '../../../core/tax-rates/tax-rate.service';
import { VoucherService } from '../../../core/vouchers/voucher.service';
import {
  CreateVoucherRequest,
  Voucher,
  VoucherAttachment,
  VoucherLineRequest,
  VoucherType,
} from '../../../core/vouchers/voucher.models';
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

interface VoucherLineFormControls {
  accountId: FormControl<number | null>;
  debitAmount: FormControl<number>;
  creditAmount: FormControl<number>;
  costCenterId: FormControl<number | null>;
  taxRateId: FormControl<number | null>;
}

type VoucherLineGroup = FormGroup<VoucherLineFormControls>;

interface VoucherLineSeed {
  accountId?: number;
  debitAmount?: number;
  creditAmount?: number;
  costCenterId?: number | null;
  taxRateId?: number | null;
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
  selector: 'app-voucher-form',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DatePickerModule,
    FileUploadModule,
    HasRightDirective,
    InputNumberModule,
    InputTextModule,
    MessageModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TextareaModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './voucher-form.html',
  styleUrl: './voucher-form.scss',
})
export class VoucherForm implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly voucherService = inject(VoucherService);
  private readonly accountService = inject(AccountService);
  private readonly costCenterService = inject(CostCenterService);
  private readonly taxRateService = inject(TaxRateService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly lookupsLoading = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly uploading = signal(false);
  readonly isEditMode = signal(false);
  readonly readOnly = signal(false);
  readonly voucherId = signal<number | null>(null);
  readonly currentVoucher = signal<Voucher | null>(null);

  readonly accountOptions = signal<{ label: string; value: number }[]>([]);
  readonly costCenterOptions = signal<{ label: string; value: number | null }[]>([
    { label: '(None)', value: null },
  ]);
  readonly taxRateOptions = signal<{ label: string; value: number | null }[]>([
    { label: '(None)', value: null },
  ]);

  readonly voucherTypeOptions = VOUCHER_TYPES.map((value) => ({ label: value, value }));

  form = this.fb.nonNullable.group({
    voucherType: this.fb.nonNullable.control<VoucherType>('Journal', [Validators.required]),
    date: this.fb.control<Date | null>(new Date(), [Validators.required]),
    narration: this.fb.nonNullable.control<string>(''),
    currencyCode: this.fb.nonNullable.control<string>('USD', [Validators.required]),
    exchangeRate: this.fb.nonNullable.control<number>(1, [Validators.required]),
    lines: this.fb.array<VoucherLineGroup>([this.createLineGroup(), this.createLineGroup()]),
  });

  get linesArray(): FormArray<VoucherLineGroup> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    this.loadLookups();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      const isEditRoute = this.route.snapshot.data['mode'] === 'edit';
      this.isEditMode.set(true);
      this.voucherId.set(id);
      this.loadVoucher(id, isEditRoute);
    }
  }

  addLine(): void {
    this.linesArray.push(this.createLineGroup());
  }

  removeLine(index: number): void {
    if (this.linesArray.length > 2) {
      this.linesArray.removeAt(index);
    }
  }

  showTaxRate(): boolean {
    const type = this.form.controls.voucherType.value;
    return type === 'Sales' || type === 'Purchase';
  }

  totalDebit(): number {
    return this.linesArray.getRawValue().reduce((sum, line) => sum + (line.debitAmount ?? 0), 0);
  }

  totalCredit(): number {
    return this.linesArray.getRawValue().reduce((sum, line) => sum + (line.creditAmount ?? 0), 0);
  }

  balanceDifference(): number {
    return Math.abs(this.totalDebit() - this.totalCredit());
  }

  isBalanced(): boolean {
    return this.totalDebit() > 0 && this.balanceDifference() < 0.005;
  }

  validLineCount(): number {
    return this.linesArray
      .getRawValue()
      .filter((line) => line.accountId && ((line.debitAmount ?? 0) > 0 || (line.creditAmount ?? 0) > 0)).length;
  }

  hasInvalidLine(): boolean {
    return this.linesArray
      .getRawValue()
      .some((line) => (line.debitAmount ?? 0) > 0 && (line.creditAmount ?? 0) > 0);
  }

  canSave(): boolean {
    return (
      !this.saving() &&
      this.form.controls.voucherType.valid &&
      this.form.controls.date.valid &&
      this.isBalanced() &&
      this.validLineCount() >= 2 &&
      !this.hasInvalidLine()
    );
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const lines: VoucherLineRequest[] = this.linesArray
      .getRawValue()
      .filter((line) => line.accountId && ((line.debitAmount ?? 0) > 0 || (line.creditAmount ?? 0) > 0))
      .map((line) => ({
        accountId: line.accountId!,
        debitAmount: line.debitAmount ?? 0,
        creditAmount: line.creditAmount ?? 0,
        costCenterId: line.costCenterId ?? null,
        taxRateId: line.taxRateId ?? null,
      }));

    const request: CreateVoucherRequest = {
      voucherType: raw.voucherType,
      date: toIsoDate(raw.date) ?? '',
      narration: raw.narration,
      currencyCode: raw.currencyCode,
      exchangeRate: raw.exchangeRate,
      lines,
    };

    this.saving.set(true);
    const id = this.voucherId();
    const call = id ? this.voucherService.update(id, request) : this.voucherService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(id ? 'Voucher updated successfully.' : 'Voucher created successfully.');
        this.router.navigate(['/vouchers']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save voucher.'));
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/vouchers']);
  }

  saveRightCode(): RightCode {
    return this.voucherId() ? RightCode.VouchersEdit : RightCode.VouchersCreate;
  }

  downloadAttachment(attachment: VoucherAttachment): void {
    const id = this.voucherId();
    if (!id) {
      return;
    }

    this.voucherService.downloadAttachment(id, attachment.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = attachment.fileName;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to download attachment.'));
      },
    });
  }

  confirmDeleteAttachment(attachment: VoucherAttachment): void {
    this.confirmationService.confirm({
      header: 'Delete Attachment',
      message: `Delete attachment "${attachment.fileName}"? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Delete' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => this.deleteAttachment(attachment),
    });
  }

  onUpload(event: FileUploadHandlerEvent, fileUpload: FileUpload): void {
    const id = this.voucherId();
    const file = event.files[0];
    if (!id || !file) {
      return;
    }

    this.uploading.set(true);
    this.voucherService.uploadAttachment(id, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.notificationService.success('Attachment uploaded successfully.');
        fileUpload.clear();
        this.refreshAttachments(id);
      },
      error: (error: HttpErrorResponse) => {
        this.uploading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to upload attachment.'));
      },
    });
  }

  private deleteAttachment(attachment: VoucherAttachment): void {
    const id = this.voucherId();
    if (!id) {
      return;
    }

    this.voucherService.deleteAttachment(id, attachment.id).subscribe({
      next: () => {
        this.notificationService.success('Attachment deleted.');
        this.refreshAttachments(id);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to delete attachment.'));
      },
    });
  }

  private refreshAttachments(id: number): void {
    this.voucherService.getById(id).subscribe({
      next: (response) => {
        if (response.data) {
          this.currentVoucher.set(response.data);
        }
      },
    });
  }

  private createLineGroup(line?: VoucherLineSeed): VoucherLineGroup {
    return this.fb.group({
      accountId: this.fb.control<number | null>(line?.accountId ?? null, [Validators.required]),
      debitAmount: this.fb.nonNullable.control<number>(line?.debitAmount ?? 0),
      creditAmount: this.fb.nonNullable.control<number>(line?.creditAmount ?? 0),
      costCenterId: this.fb.control<number | null>(line?.costCenterId ?? null),
      taxRateId: this.fb.control<number | null>(line?.taxRateId ?? null),
    });
  }

  private loadLookups(): void {
    this.lookupsLoading.set(true);
    forkJoin({
      accounts: this.accountService.getAll(),
      costCenters: this.costCenterService.getAll(),
      taxRates: this.taxRateService.getAll(),
    }).subscribe({
      next: ({ accounts, costCenters, taxRates }) => {
        this.lookupsLoading.set(false);
        this.accountOptions.set(
          (accounts.data ?? []).map((account) => ({ label: `${account.code} - ${account.name}`, value: account.id })),
        );
        this.costCenterOptions.set([
          { label: '(None)', value: null },
          ...(costCenters.data ?? []).map((cc) => ({ label: cc.name, value: cc.id as number | null })),
        ]);
        this.taxRateOptions.set([
          { label: '(None)', value: null },
          ...(taxRates.data ?? []).map((tr) => ({
            label: `${tr.name} (${tr.percentage}%)`,
            value: tr.id as number | null,
          })),
        ]);
      },
      error: (error: HttpErrorResponse) => {
        this.lookupsLoading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load lookup data.'));
      },
    });
  }

  private loadVoucher(id: number, isEditRoute: boolean): void {
    this.loading.set(true);
    this.voucherService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        const voucher = response.data;
        if (!voucher) {
          this.notificationService.error('Voucher not found.');
          this.router.navigate(['/vouchers']);
          return;
        }
        if (isEditRoute && voucher.status !== 'Draft') {
          this.notificationService.error('Only draft vouchers can be edited.');
          this.router.navigate(['/vouchers']);
          return;
        }

        this.readOnly.set(!isEditRoute || voucher.status !== 'Draft');
        this.currentVoucher.set(voucher);
        this.applyVoucher(voucher);

        if (this.readOnly()) {
          this.form.disable();
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load voucher.'));
        this.router.navigate(['/vouchers']);
      },
    });
  }

  private applyVoucher(voucher: Voucher): void {
    this.form.patchValue({
      voucherType: voucher.voucherType,
      date: new Date(voucher.date),
      narration: voucher.narration,
      currencyCode: voucher.currencyCode,
      exchangeRate: voucher.exchangeRate,
    });
    this.form.controls.voucherType.disable();

    this.linesArray.clear();
    for (const line of voucher.lines) {
      this.linesArray.push(
        this.createLineGroup({
          accountId: line.accountId,
          debitAmount: line.debitAmount,
          creditAmount: line.creditAmount,
          costCenterId: line.costCenterId,
          taxRateId: line.taxRateId,
        }),
      );
    }
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
