import { Component, OnInit, inject, input, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { PartnerPaymentService } from '../../core/partner-payments/partner-payment.service';
import { PartnerPayment, PartnerPaymentAllocationRequest, PartnerPaymentRequest } from '../../core/partner-payments/partner-payment.models';
import { BusinessPartnerService } from '../../core/business-partners/business-partner.service';
import { AccountService } from '../../core/accounts/account.service';
import { InvoiceService } from '../../core/invoices/invoice.service';
import { InvoiceType } from '../../core/invoices/invoice.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { PartnerPaymentConfig } from './partner-payment-config';

interface AllocationRowControls {
  invoiceHeaderId: FormControl<number>;
  invoiceNo: FormControl<string>;
  outstandingAmount: FormControl<number>;
  allocatedAmount: FormControl<number>;
}

type AllocationRow = FormGroup<AllocationRowControls>;

function toIsoDate(date: Date | null): string {
  if (!date) return '';
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-partner-payment-form',
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    ButtonModule,
    CardModule,
    DatePickerModule,
    HasRightDirective,
    InputNumberModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TextareaModule,
  ],
  templateUrl: './partner-payment-form.html',
  styleUrl: './partner-payment-form.scss',
})
export class PartnerPaymentForm implements OnInit {
  readonly config = input.required<PartnerPaymentConfig>();

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly paymentService = inject(PartnerPaymentService);
  private readonly partnerService = inject(BusinessPartnerService);
  private readonly accountService = inject(AccountService);
  private readonly invoiceService = inject(InvoiceService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly readOnly = signal(false);
  readonly paymentId = signal<number | null>(null);
  readonly current = signal<PartnerPayment | null>(null);

  readonly partnerOptions = signal<{ label: string; value: number }[]>([]);
  readonly accountOptions = signal<{ label: string; value: number }[]>([]);

  form = this.fb.nonNullable.group({
    partnerId: this.fb.control<number | null>(null, [Validators.required]),
    date: this.fb.control<Date | null>(new Date(), [Validators.required]),
    bankOrCashAccountId: this.fb.control<number | null>(null, [Validators.required]),
    totalAmount: this.fb.nonNullable.control<number>(0, [Validators.required]),
    narration: this.fb.control<string | null>(null),
    allocations: this.fb.array<AllocationRow>([]),
  });

  get allocationsArray(): FormArray<AllocationRow> {
    return this.form.controls.allocations;
  }

  ngOnInit(): void {
    this.loadLookups();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.paymentId.set(Number(idParam));
      this.loadPayment(Number(idParam));
    }
  }

  onPartnerChange(): void {
    const partnerId = this.form.controls.partnerId.value;
    if (!partnerId || this.readOnly()) return;
    this.loadOutstandingInvoices(partnerId);
  }

  allocatedTotal(): number {
    return this.allocationsArray.getRawValue().reduce((sum, row) => sum + (row.allocatedAmount ?? 0), 0);
  }

  amountMismatch(): boolean {
    return Math.abs(this.allocatedTotal() - (this.form.controls.totalAmount.value ?? 0)) > 0.005;
  }

  canSave(): boolean {
    return (
      !this.saving() &&
      this.form.controls.partnerId.valid &&
      this.form.controls.bankOrCashAccountId.valid &&
      this.form.controls.totalAmount.value > 0 &&
      !this.amountMismatch() &&
      this.allocationsArray.getRawValue().some((row) => row.allocatedAmount > 0)
    );
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const allocations: PartnerPaymentAllocationRequest[] = this.allocationsArray
      .getRawValue()
      .filter((row) => row.allocatedAmount > 0)
      .map((row) => ({ invoiceHeaderId: row.invoiceHeaderId, allocatedAmount: row.allocatedAmount }));

    const request: PartnerPaymentRequest = {
      direction: this.config().direction,
      partnerId: raw.partnerId!,
      date: toIsoDate(raw.date),
      bankOrCashAccountId: raw.bankOrCashAccountId!,
      totalAmount: raw.totalAmount,
      narration: raw.narration,
      allocations,
    };

    this.saving.set(true);
    this.paymentService.create(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success('Created successfully.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save.'));
      },
    });
  }

  cancelForm(): void {
    this.router.navigate([this.config().listRoute]);
  }

  submitForApproval(): void {
    const id = this.paymentId();
    if (!id) return;
    this.paymentService.submit(id).subscribe({
      next: () => {
        this.notificationService.success('Submitted for approval.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(): void {
    const id = this.paymentId();
    if (!id) return;
    this.paymentService.approve(id).subscribe({
      next: () => {
        this.notificationService.success('Approved.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(): void {
    const id = this.paymentId();
    if (!id) return;
    this.paymentService.reject(id).subscribe({
      next: () => {
        this.notificationService.success('Rejected.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject.'));
      },
    });
  }

  private loadLookups(): void {
    const partnerType = this.config().direction === 'CustomerReceipt' ? 'Customer' : 'Supplier';

    forkJoin({
      partners: this.partnerService.getAll(partnerType),
      accounts: this.accountService.getAll(),
    }).subscribe({
      next: ({ partners, accounts }) => {
        this.partnerOptions.set((partners.data ?? []).map((p) => ({ label: `${p.code} - ${p.name}`, value: p.id })));
        this.accountOptions.set((accounts.data ?? []).map((a) => ({ label: `${a.code} - ${a.name}`, value: a.id })));
      },
    });
  }

  private loadOutstandingInvoices(partnerId: number): void {
    const primaryType: InvoiceType = this.config().direction === 'CustomerReceipt' ? 'SalesInvoice' : 'PurchaseInvoice';
    const returnType: InvoiceType = this.config().direction === 'CustomerReceipt' ? 'SaleReturn' : 'PurchaseReturn';

    forkJoin({
      primary: this.invoiceService.getAll({ invoiceType: primaryType, partnerId, pageNumber: 1, pageSize: 200 }),
      returns: this.invoiceService.getAll({ invoiceType: returnType, partnerId, pageNumber: 1, pageSize: 200 }),
    }).subscribe({
      next: ({ primary, returns }) => {
        const outstanding = [...(primary.data?.items ?? []), ...(returns.data?.items ?? [])].filter(
          (i) => i.status === 'Posted' && (i.paymentStatus === 'Unpaid' || i.paymentStatus === 'PartiallyPaid'),
        );

        this.allocationsArray.clear();
        for (const invoice of outstanding) {
          this.allocationsArray.push(
            this.fb.group({
              invoiceHeaderId: this.fb.nonNullable.control(invoice.id),
              invoiceNo: this.fb.nonNullable.control(invoice.invoiceNo),
              outstandingAmount: this.fb.nonNullable.control(invoice.outstandingAmount),
              allocatedAmount: this.fb.nonNullable.control(0),
            }),
          );
        }
      },
    });
  }

  private loadPayment(id: number): void {
    this.loading.set(true);
    this.paymentService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        const payment = response.data;
        if (!payment) {
          this.notificationService.error('Not found.');
          this.router.navigate([this.config().listRoute]);
          return;
        }

        this.readOnly.set(true);
        this.current.set(payment);
        this.form.patchValue({
          partnerId: payment.partnerId,
          date: new Date(payment.date),
          bankOrCashAccountId: payment.bankOrCashAccountId,
          totalAmount: payment.totalAmount,
          narration: payment.narration,
        });

        this.allocationsArray.clear();
        for (const allocation of payment.allocations) {
          this.allocationsArray.push(
            this.fb.group({
              invoiceHeaderId: this.fb.nonNullable.control(allocation.invoiceHeaderId),
              invoiceNo: this.fb.nonNullable.control(allocation.invoiceNo),
              outstandingAmount: this.fb.nonNullable.control(allocation.invoiceOutstandingAmount),
              allocatedAmount: this.fb.nonNullable.control(allocation.allocatedAmount),
            }),
          );
        }

        this.form.disable();
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load.'));
        this.router.navigate([this.config().listRoute]);
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
