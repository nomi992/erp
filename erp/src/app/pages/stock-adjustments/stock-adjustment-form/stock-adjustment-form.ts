import { Component, OnInit, inject, signal } from '@angular/core';
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
import { StockAdjustmentService } from '../../../core/stock-adjustments/stock-adjustment.service';
import {
  AdjustmentDirection,
  AdjustmentReasonCode,
  StockAdjustment,
  StockAdjustmentLineRequest,
  StockAdjustmentRequest,
} from '../../../core/stock-adjustments/stock-adjustment.models';
import { ProductService } from '../../../core/products/product.service';
import { WarehouseService } from '../../../core/warehouses/warehouse.service';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

interface LineFormControls {
  productVariantId: FormControl<number | null>;
  direction: FormControl<AdjustmentDirection>;
  baseQty: FormControl<number>;
  unitCost: FormControl<number | null>;
}

type LineGroup = FormGroup<LineFormControls>;

const REASON_CODES: AdjustmentReasonCode[] = ['Damage', 'Expiry', 'Loss', 'CountIncrease', 'CountDecrease', 'OpeningBalance', 'Other'];

function toIsoDate(date: Date | null): string {
  if (!date) return '';
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-stock-adjustment-form',
  imports: [
    ReactiveFormsModule,
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
  templateUrl: './stock-adjustment-form.html',
  styleUrl: './stock-adjustment-form.scss',
})
export class StockAdjustmentForm implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly adjustmentService = inject(StockAdjustmentService);
  private readonly productService = inject(ProductService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly readOnly = signal(false);
  readonly adjustmentId = signal<number | null>(null);
  readonly current = signal<StockAdjustment | null>(null);

  readonly warehouseOptions = signal<{ label: string; value: number }[]>([]);
  readonly variantOptions = signal<{ label: string; value: number }[]>([]);
  readonly reasonCodeOptions = REASON_CODES.map((value) => ({ label: value, value }));
  readonly directionOptions: { label: string; value: AdjustmentDirection }[] = [
    { label: 'Increase', value: 'Increase' },
    { label: 'Decrease', value: 'Decrease' },
  ];

  form = this.fb.nonNullable.group({
    warehouseId: this.fb.control<number | null>(null, [Validators.required]),
    date: this.fb.control<Date | null>(new Date(), [Validators.required]),
    reasonCode: this.fb.nonNullable.control<AdjustmentReasonCode>('CountIncrease', [Validators.required]),
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
      this.adjustmentId.set(Number(idParam));
      this.loadAdjustment(Number(idParam));
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

  requiresUnitCost(group: LineGroup): boolean {
    return group.controls.direction.value === 'Increase';
  }

  canSave(): boolean {
    return (
      !this.saving() &&
      this.form.controls.warehouseId.valid &&
      this.linesArray
        .getRawValue()
        .some((l) => l.productVariantId && l.baseQty > 0 && (l.direction === 'Decrease' || (l.unitCost ?? 0) > 0))
    );
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const lines: StockAdjustmentLineRequest[] = this.linesArray
      .getRawValue()
      .filter((l) => l.productVariantId && l.baseQty > 0)
      .map((l) => ({
        productVariantId: l.productVariantId!,
        direction: l.direction,
        baseQty: l.baseQty,
        unitCost: l.direction === 'Increase' ? l.unitCost : null,
      }));

    const request: StockAdjustmentRequest = {
      warehouseId: raw.warehouseId!,
      date: toIsoDate(raw.date),
      reasonCode: raw.reasonCode,
      narration: raw.narration,
      lines,
    };

    this.saving.set(true);
    this.adjustmentService.create(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success('Stock adjustment created.');
        this.router.navigate(['/stock-adjustments']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save.'));
      },
    });
  }

  cancelForm(): void {
    this.router.navigate(['/stock-adjustments']);
  }

  submitForApproval(): void {
    const id = this.adjustmentId();
    if (!id) return;
    this.adjustmentService.submit(id).subscribe({
      next: () => {
        this.notificationService.success('Submitted for approval.');
        this.router.navigate(['/stock-adjustments']);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(): void {
    const id = this.adjustmentId();
    if (!id) return;
    this.adjustmentService.approve(id).subscribe({
      next: () => {
        this.notificationService.success('Approved.');
        this.router.navigate(['/stock-adjustments']);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(): void {
    const id = this.adjustmentId();
    if (!id) return;
    this.adjustmentService.reject(id).subscribe({
      next: () => {
        this.notificationService.success('Rejected.');
        this.router.navigate(['/stock-adjustments']);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject.'));
      },
    });
  }

  private createLineGroup(): LineGroup {
    return this.fb.group({
      productVariantId: this.fb.control<number | null>(null, [Validators.required]),
      direction: this.fb.nonNullable.control<AdjustmentDirection>('Increase'),
      baseQty: this.fb.nonNullable.control<number>(1),
      unitCost: this.fb.control<number | null>(null),
    });
  }

  private loadLookups(): void {
    forkJoin({
      warehouses: this.warehouseService.getAll(),
      products: this.productService.getAll(),
    }).subscribe({
      next: ({ warehouses, products }) => {
        this.warehouseOptions.set((warehouses.data ?? []).map((w) => ({ label: w.name, value: w.id })));

        const options: { label: string; value: number }[] = [];
        // Non-stock-tracked products (services) have no StockBalance to adjust — the backend
        // rejects them outright, so keep them out of the picker rather than surfacing that as a save error.
        for (const product of (products.data ?? []).filter((p) => p.isStockTracked)) {
          for (const variant of product.variants.filter((v) => v.isActive)) {
            options.push({ label: `${product.name} - ${variant.name} (${variant.variantCode})`, value: variant.id });
          }
        }
        this.variantOptions.set(options);
      },
    });
  }

  private loadAdjustment(id: number): void {
    this.loading.set(true);
    this.adjustmentService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        const adjustment = response.data;
        if (!adjustment) {
          this.notificationService.error('Not found.');
          this.router.navigate(['/stock-adjustments']);
          return;
        }

        this.readOnly.set(true);
        this.current.set(adjustment);
        this.form.patchValue({
          warehouseId: adjustment.warehouseId,
          date: new Date(adjustment.date),
          reasonCode: adjustment.reasonCode,
          narration: adjustment.narration,
        });

        this.linesArray.clear();
        for (const line of adjustment.lines) {
          this.linesArray.push(
            this.fb.group({
              productVariantId: this.fb.control<number | null>(line.productVariantId),
              direction: this.fb.nonNullable.control<AdjustmentDirection>(line.direction),
              baseQty: this.fb.nonNullable.control<number>(line.baseQty),
              unitCost: this.fb.control<number | null>(line.unitCost),
            }),
          );
        }
        this.form.disable();
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load.'));
        this.router.navigate(['/stock-adjustments']);
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
