import { Component, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InvoiceService } from '../../core/invoices/invoice.service';
import { Invoice, InvoiceListItem, InvoicePaymentStatus, InvoiceStatus } from '../../core/invoices/invoice.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { AuthService } from '../../core/auth/auth.service';
import { TenancyService } from '../../core/tenancy/tenancy.service';
import { QzTrayService } from '../../core/printing/qz-tray.service';
import { PrinterSelectDialog } from '../printing/printer-select-dialog';
import { InvoiceTypeConfig } from './invoice-type-config';
import { buildInvoicePrintHtml } from './invoice-print-template';

const STATUSES: InvoiceStatus[] = ['Draft', 'PendingApproval', 'Posted', 'Rejected', 'Cancelled'];

function toIsoDate(date: Date | null): string | undefined {
  if (!date) return undefined;
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-invoice-list',
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
    PrinterSelectDialog,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './invoice-list.html',
  styleUrl: './invoice-list.scss',
})
export class InvoiceList {
  readonly config = input.required<InvoiceTypeConfig>();

  private readonly fb = inject(FormBuilder);
  private readonly invoiceService = inject(InvoiceService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly tenancyService = inject(TenancyService);
  private readonly qzTrayService = inject(QzTrayService);

  readonly loading = signal(false);
  readonly invoices = signal<InvoiceListItem[]>([]);
  readonly totalRecords = signal(0);
  readonly rows = signal(25);
  readonly searchTerm = signal('');

  readonly printerDialogVisible = signal(false);
  readonly pendingPrintInvoice = signal<Invoice | null>(null);

  private searchDebounceHandle?: ReturnType<typeof setTimeout>;
  private sortField?: string;
  private sortOrder?: number | null;

  readonly statusOptions = [
    { label: 'All Statuses', value: null },
    ...STATUSES.map((value) => ({ label: value, value })),
  ];

  filterForm = this.fb.group({
    status: this.fb.control<InvoiceStatus | null>(null),
    from: this.fb.control<Date | null>(null),
    to: this.fb.control<Date | null>(null),
  });

  load(event?: TableLazyLoadEvent): void {
    const raw = this.filterForm.getRawValue();
    const first = event?.first ?? 0;
    const rows = event?.rows ?? this.rows();
    const pageNumber = Math.floor(first / rows) + 1;

    if (event) {
      this.sortField = typeof event.sortField === 'string' ? event.sortField : undefined;
      this.sortOrder = event.sortOrder;
    }

    this.loading.set(true);
    this.invoiceService
      .getAll({
        invoiceType: this.config().invoiceType,
        status: raw.status ?? undefined,
        from: toIsoDate(raw.from),
        to: toIsoDate(raw.to),
        search: this.searchTerm() || undefined,
        sortBy: this.sortField,
        sortDirection: this.sortOrder === 1 ? 'asc' : this.sortOrder === -1 ? 'desc' : undefined,
        pageNumber,
        pageSize: rows,
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          this.invoices.set(response.data?.items ?? []);
          this.totalRecords.set(response.data?.totalCount ?? 0);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          this.notificationService.error(this.extractErrorMessage(error, `Unable to load ${this.config().title.toLowerCase()}.`));
        },
      });
  }

  onSearchChange(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
    if (this.searchDebounceHandle) {
      clearTimeout(this.searchDebounceHandle);
    }
    this.searchDebounceHandle = setTimeout(() => this.load(), 300);
  }

  clearFilters(): void {
    this.filterForm.reset({ status: null, from: null, to: null });
    this.searchTerm.set('');
    this.load();
  }

  statusSeverity(status: InvoiceStatus): 'secondary' | 'warn' | 'success' | 'danger' | 'info' {
    switch (status) {
      case 'Draft':
        return 'secondary';
      case 'PendingApproval':
        return 'warn';
      case 'Posted':
        return 'success';
      case 'Rejected':
      case 'Cancelled':
        return 'danger';
    }
  }

  paymentStatusSeverity(status: InvoicePaymentStatus): 'secondary' | 'warn' | 'success' | 'danger' {
    switch (status) {
      case 'Unpaid':
        return 'secondary';
      case 'PartiallyPaid':
        return 'warn';
      case 'Paid':
        return 'success';
      case 'Overdue':
        return 'danger';
    }
  }

  create(): void {
    this.router.navigate([this.config().formRoute, 'new']);
  }

  view(item: InvoiceListItem): void {
    this.router.navigate([this.config().formRoute, item.id]);
  }

  edit(item: InvoiceListItem): void {
    this.router.navigate([this.config().formRoute, item.id, 'edit']);
  }

  submit(item: InvoiceListItem): void {
    this.invoiceService.submit(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.invoiceNo} submitted for approval.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(item: InvoiceListItem): void {
    this.invoiceService.approve(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.invoiceNo} approved.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(item: InvoiceListItem): void {
    this.invoiceService.reject(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.invoiceNo} rejected.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject.'));
      },
    });
  }

  confirmCancel(item: InvoiceListItem): void {
    this.confirmationService.confirm({
      header: `Cancel ${this.config().singularLabel}`,
      message: `Cancel "${item.invoiceNo}"? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Cancel It' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Back' },
      accept: () => this.cancel(item),
    });
  }

  print(item: InvoiceListItem): void {
    this.invoiceService.getById(item.id).subscribe({
      next: (response) => {
        const invoice = response.data;
        if (!invoice) {
          this.notificationService.error('Unable to load invoice for printing.');
          return;
        }
        this.pendingPrintInvoice.set(invoice);
        this.printerDialogVisible.set(true);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load invoice for printing.'));
      },
    });
  }

  onPrinterChosen(printerName: string): void {
    const invoice = this.pendingPrintInvoice();
    if (!invoice) return;

    const html = buildInvoicePrintHtml(invoice, this.config(), {
      companyName: this.tenancyService.effectiveTenantName(),
      branchName: this.tenancyService.currentBranch()?.name ?? null,
      printedBy: this.authService.username(),
    });

    this.qzTrayService.printHtml(printerName, html).then(
      () => this.notificationService.success(`${invoice.invoiceNo} sent to ${printerName}.`),
      (error: unknown) => {
        this.notificationService.error(error instanceof Error ? error.message : 'Unable to print.');
      },
    );
  }

  private cancel(item: InvoiceListItem): void {
    this.invoiceService.cancel(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.invoiceNo} cancelled.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to cancel.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
