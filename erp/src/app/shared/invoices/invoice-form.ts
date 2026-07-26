import { Component, OnInit, inject, input, signal } from '@angular/core';
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
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { InvoiceService } from '../../core/invoices/invoice.service';
import { Invoice, InvoiceLineRequest, InvoiceRequest, PaymentMode } from '../../core/invoices/invoice.models';
import { ProductService } from '../../core/products/product.service';
import { ProductVariantPrice } from '../../core/products/product.models';
import { BusinessPartnerService } from '../../core/business-partners/business-partner.service';
import { WarehouseService } from '../../core/warehouses/warehouse.service';
import { UnitOfMeasureService } from '../../core/units-of-measure/unit-of-measure.service';
import { TaxRateService } from '../../core/tax-rates/tax-rate.service';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { InvoiceTypeConfig } from './invoice-type-config';

interface LineFormControls {
  productVariantId: FormControl<number | null>;
  unitOfMeasureId: FormControl<number | null>;
  qty: FormControl<number>;
  unitAmount: FormControl<number>;
  taxRateId: FormControl<number | null>;
  referenceInvoiceLineId: FormControl<number | null>;
}

type LineGroup = FormGroup<LineFormControls>;

interface VariantMeta {
  productId: number;
  productName: string;
  variantName: string;
  baseUnitOfMeasureId: number;
  prices: ProductVariantPrice[];
}

function toIsoDate(date: Date | null): string | undefined {
  if (!date) return undefined;
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-invoice-form',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DatePickerModule,
    HasRightDirective,
    InputNumberModule,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TextareaModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './invoice-form.html',
  styleUrl: './invoice-form.scss',
})
export class InvoiceForm implements OnInit {
  readonly config = input.required<InvoiceTypeConfig>();

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly invoiceService = inject(InvoiceService);
  private readonly productService = inject(ProductService);
  private readonly partnerService = inject(BusinessPartnerService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly unitService = inject(UnitOfMeasureService);
  private readonly taxRateService = inject(TaxRateService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly lookupsLoading = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly readOnly = signal(false);
  readonly invoiceId = signal<number | null>(null);
  readonly current = signal<Invoice | null>(null);
  readonly referenceDoc = signal<Invoice | null>(null);

  readonly partnerOptions = signal<{ label: string; value: number }[]>([]);
  readonly warehouseOptions = signal<{ label: string; value: number }[]>([]);
  readonly variantOptions = signal<{ label: string; value: number }[]>([]);
  readonly variantMeta = signal<Record<number, VariantMeta>>({});
  readonly uomOptions = signal<{ label: string; value: number }[]>([]);
  readonly taxRateOptions = signal<{ label: string; value: number | null; percentage: number }[]>([
    { label: '(None)', value: null, percentage: 0 },
  ]);
  readonly referenceOptions = signal<{ label: string; value: number }[]>([]);

  readonly paymentModeOptions: { label: string; value: PaymentMode }[] = [
    { label: 'Cash', value: 'Cash' },
    { label: 'Credit', value: 'Credit' },
  ];

  form = this.fb.nonNullable.group({
    partnerId: this.fb.control<number | null>(null, [Validators.required]),
    warehouseId: this.fb.control<number | null>(null, [Validators.required]),
    date: this.fb.control<Date | null>(new Date(), [Validators.required]),
    externalReferenceNo: this.fb.control<string | null>(null),
    paymentMode: this.fb.nonNullable.control<PaymentMode>('Credit'),
    paymentTermDays: this.fb.nonNullable.control<number>(0),
    requestedDeliveryDate: this.fb.control<Date | null>(null),
    referenceInvoiceId: this.fb.control<number | null>(null),
    narration: this.fb.control<string | null>(null),
    lines: this.fb.array<LineGroup>([]),
  });

  get linesArray(): FormArray<LineGroup> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    this.loadLookups();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      const isEditRoute = this.route.snapshot.data['mode'] === 'edit';
      this.invoiceId.set(id);
      this.loadInvoice(id, isEditRoute);
    } else {
      this.addLine();
    }
  }

  addLine(): void {
    this.linesArray.push(this.createLineGroup());
  }

  removeLine(index: number): void {
    if (this.linesArray.length > 1) {
      this.linesArray.removeAt(index);
    }
  }

  onVariantChange(index: number): void {
    const group = this.linesArray.at(index);
    const variantId = group.controls.productVariantId.value;
    if (!variantId) return;

    const meta = this.variantMeta()[variantId];
    if (!meta) return;

    if (!group.controls.unitOfMeasureId.value) {
      group.controls.unitOfMeasureId.setValue(meta.baseUnitOfMeasureId);
    }

    if (!group.controls.unitAmount.value) {
      const priceType = this.config().isPurchaseSide ? 'Purchase' : 'Sale';
      const price = meta.prices.find((p) => p.priceType === priceType);
      if (price) {
        group.controls.unitAmount.setValue(price.amount);
      }
    }
  }

  onReferenceChange(): void {
    const refId = this.form.controls.referenceInvoiceId.value;
    this.referenceDoc.set(null);
    if (!refId) return;

    this.invoiceService.getById(refId).subscribe({
      next: (response) => this.referenceDoc.set(response.data ?? null),
    });
  }

  copyLinesFromReference(): void {
    const ref = this.referenceDoc();
    if (!ref) return;

    // Prefills the full original quantity/price from the referenced order or invoice line — the
    // user reduces it manually for a partial fulfillment/return; the backend re-validates against
    // the referenced line's remaining BaseQty regardless of what's submitted here.
    this.linesArray.clear();
    for (const line of ref.lines) {
      this.linesArray.push(
        this.createLineGroup({
          productVariantId: line.productVariantId,
          unitOfMeasureId: line.unitOfMeasureId,
          qty: line.qty,
          unitAmount: line.unitAmount,
          taxRateId: line.taxRateId,
          referenceInvoiceLineId: line.id,
        }),
      );
    }
  }

  lineTotal(group: LineGroup): number {
    const raw = group.getRawValue();
    const net = (raw.qty ?? 0) * (raw.unitAmount ?? 0);
    const tax = this.lineTax(group);
    return net + tax;
  }

  lineTax(group: LineGroup): number {
    const raw = group.getRawValue();
    const rate = this.taxRateOptions().find((t) => t.value === raw.taxRateId);
    if (!rate) return 0;
    return Math.round((raw.qty ?? 0) * (raw.unitAmount ?? 0) * rate.percentage) / 100;
  }

  totalNet(): number {
    return this.linesArray.controls.reduce((sum, g) => sum + g.getRawValue().qty * g.getRawValue().unitAmount, 0);
  }

  totalTax(): number {
    return this.linesArray.controls.reduce((sum, g) => sum + this.lineTax(g), 0);
  }

  totalAmount(): number {
    return this.totalNet() + this.totalTax();
  }

  validLineCount(): number {
    return this.linesArray.getRawValue().filter((l) => l.productVariantId && l.qty > 0).length;
  }

  canSave(): boolean {
    return (
      !this.saving() &&
      this.form.controls.partnerId.valid &&
      this.form.controls.warehouseId.valid &&
      this.form.controls.date.valid &&
      this.validLineCount() > 0
    );
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const lines: InvoiceLineRequest[] = this.linesArray
      .getRawValue()
      .filter((l) => l.productVariantId && l.qty > 0)
      .map((l) => ({
        productVariantId: l.productVariantId!,
        unitOfMeasureId: l.unitOfMeasureId!,
        qty: l.qty,
        unitAmount: l.unitAmount,
        taxRateId: l.taxRateId,
        referenceInvoiceLineId: l.referenceInvoiceLineId,
      }));

    const request: InvoiceRequest = {
      invoiceType: this.config().invoiceType,
      externalReferenceNo: raw.externalReferenceNo,
      partnerId: raw.partnerId!,
      referenceInvoiceId: raw.referenceInvoiceId,
      warehouseId: raw.warehouseId!,
      date: toIsoDate(raw.date) ?? '',
      paymentMode: raw.paymentMode,
      paymentTermDays: raw.paymentTermDays,
      requestedDeliveryDate: toIsoDate(raw.requestedDeliveryDate) ?? null,
      narration: raw.narration,
      lines,
    };

    this.saving.set(true);
    const id = this.invoiceId();
    const call = id ? this.invoiceService.update(id, request) : this.invoiceService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(id ? 'Updated successfully.' : 'Created successfully.');
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
    const id = this.invoiceId();
    if (!id) return;

    this.invoiceService.submit(id).subscribe({
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
    const id = this.invoiceId();
    if (!id) return;

    this.invoiceService.approve(id).subscribe({
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
    const id = this.invoiceId();
    if (!id) return;

    this.invoiceService.reject(id).subscribe({
      next: () => {
        this.notificationService.success('Rejected.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject.'));
      },
    });
  }

  confirmCancelDocument(): void {
    this.confirmationService.confirm({
      header: `Cancel ${this.config().singularLabel}`,
      message: `Cancel this ${this.config().singularLabel.toLowerCase()}? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Cancel It' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Back' },
      accept: () => this.cancelDocument(),
    });
  }

  private cancelDocument(): void {
    const id = this.invoiceId();
    if (!id) return;

    this.invoiceService.cancel(id).subscribe({
      next: () => {
        this.notificationService.success('Cancelled.');
        this.router.navigate([this.config().listRoute]);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to cancel.'));
      },
    });
  }

  private createLineGroup(seed?: Partial<InvoiceLineRequest>): LineGroup {
    return this.fb.group({
      productVariantId: this.fb.control<number | null>(seed?.productVariantId ?? null, [Validators.required]),
      unitOfMeasureId: this.fb.control<number | null>(seed?.unitOfMeasureId ?? null, [Validators.required]),
      qty: this.fb.nonNullable.control<number>(seed?.qty ?? 1),
      unitAmount: this.fb.nonNullable.control<number>(seed?.unitAmount ?? 0),
      taxRateId: this.fb.control<number | null>(seed?.taxRateId ?? null),
      referenceInvoiceLineId: this.fb.control<number | null>(seed?.referenceInvoiceLineId ?? null),
    });
  }

  private loadLookups(): void {
    this.lookupsLoading.set(true);

    const partnerType = this.config().isPurchaseSide ? 'Supplier' : 'Customer';

    forkJoin({
      partners: this.partnerService.getAll(partnerType),
      warehouses: this.warehouseService.getAll(),
      products: this.productService.getAll(),
      units: this.unitService.getAll(),
      taxRates: this.taxRateService.getAll(),
    }).subscribe({
      next: ({ partners, warehouses, products, units, taxRates }) => {
        this.lookupsLoading.set(false);

        this.partnerOptions.set((partners.data ?? []).map((p) => ({ label: `${p.code} - ${p.name}`, value: p.id })));
        this.warehouseOptions.set((warehouses.data ?? []).map((w) => ({ label: w.name, value: w.id })));
        this.uomOptions.set((units.data ?? []).map((u) => ({ label: `${u.code} - ${u.name}`, value: u.id })));
        this.taxRateOptions.set([
          { label: '(None)', value: null, percentage: 0 },
          ...(taxRates.data ?? []).map((t) => ({ label: `${t.name} (${t.percentage}%)`, value: t.id as number | null, percentage: t.percentage })),
        ]);

        const meta: Record<number, VariantMeta> = {};
        const options: { label: string; value: number }[] = [];
        for (const product of products.data ?? []) {
          for (const variant of product.variants.filter((v) => v.isActive)) {
            options.push({ label: `${product.name} - ${variant.name} (${variant.variantCode})`, value: variant.id });
            meta[variant.id] = {
              productId: product.id,
              productName: product.name,
              variantName: variant.name,
              baseUnitOfMeasureId: product.baseUnitOfMeasureId,
              prices: variant.prices,
            };
          }
        }
        this.variantOptions.set(options);
        this.variantMeta.set(meta);

        this.loadReferenceOptions();
      },
      error: (error: HttpErrorResponse) => {
        this.lookupsLoading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load lookup data.'));
      },
    });
  }

  private loadReferenceOptions(): void {
    const refType = this.config().referenceInvoiceType;
    if (!refType) return;

    this.invoiceService.getAll({ invoiceType: refType, status: 'Posted', pageNumber: 1, pageSize: 200 }).subscribe({
      next: (response) => {
        this.referenceOptions.set((response.data?.items ?? []).map((i) => ({ label: `${i.invoiceNo} - ${i.partnerName}`, value: i.id })));
      },
    });
  }

  private loadInvoice(id: number, isEditRoute: boolean): void {
    this.loading.set(true);
    this.invoiceService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        const invoice = response.data;
        if (!invoice) {
          this.notificationService.error('Not found.');
          this.router.navigate([this.config().listRoute]);
          return;
        }

        if (isEditRoute && invoice.status !== 'Draft') {
          this.notificationService.error('Only draft documents can be edited.');
          this.router.navigate([this.config().listRoute]);
          return;
        }

        this.readOnly.set(!isEditRoute || invoice.status !== 'Draft');
        this.current.set(invoice);
        this.applyInvoice(invoice);

        if (this.readOnly()) {
          this.form.disable();
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load.'));
        this.router.navigate([this.config().listRoute]);
      },
    });
  }

  private applyInvoice(invoice: Invoice): void {
    this.form.patchValue({
      partnerId: invoice.partnerId,
      warehouseId: invoice.warehouseId,
      date: new Date(invoice.date),
      externalReferenceNo: invoice.externalReferenceNo,
      paymentMode: invoice.paymentMode,
      paymentTermDays: invoice.paymentTermDays,
      requestedDeliveryDate: this.config().isOrder ? new Date(invoice.dueDate) : null,
      referenceInvoiceId: invoice.referenceInvoiceId,
      narration: invoice.narration,
    });

    this.linesArray.clear();
    for (const line of invoice.lines) {
      this.linesArray.push(
        this.createLineGroup({
          productVariantId: line.productVariantId,
          unitOfMeasureId: line.unitOfMeasureId,
          qty: line.qty,
          unitAmount: line.unitAmount,
          taxRateId: line.taxRateId,
          referenceInvoiceLineId: line.referenceInvoiceLineId,
        }),
      );
    }
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
