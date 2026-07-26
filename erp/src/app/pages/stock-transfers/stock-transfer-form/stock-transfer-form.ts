import { Component, OnInit, inject, signal } from '@angular/core';
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
import { StockTransferService } from '../../../core/stock-transfers/stock-transfer.service';
import { StockTransfer, StockTransferLineRequest, StockTransferReceiveLineRequest, StockTransferRequest } from '../../../core/stock-transfers/stock-transfer.models';
import { ProductService } from '../../../core/products/product.service';
import { WarehouseService } from '../../../core/warehouses/warehouse.service';
import { UnitOfMeasureService } from '../../../core/units-of-measure/unit-of-measure.service';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

interface LineFormControls {
  productVariantId: FormControl<number | null>;
  unitOfMeasureId: FormControl<number | null>;
  qty: FormControl<number>;
}

type LineGroup = FormGroup<LineFormControls>;

interface ReceiveRowControls {
  lineId: FormControl<number>;
  label: FormControl<string>;
  remaining: FormControl<number>;
  receiveNow: FormControl<number>;
}

type ReceiveRow = FormGroup<ReceiveRowControls>;

function toIsoDate(date: Date | null): string {
  if (!date) return '';
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-stock-transfer-form',
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
  templateUrl: './stock-transfer-form.html',
  styleUrl: './stock-transfer-form.scss',
})
export class StockTransferForm implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly transferService = inject(StockTransferService);
  private readonly productService = inject(ProductService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly unitService = inject(UnitOfMeasureService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly receiving = signal(false);
  readonly readOnly = signal(false);
  readonly transferId = signal<number | null>(null);
  readonly current = signal<StockTransfer | null>(null);

  readonly warehouseOptions = signal<{ label: string; value: number }[]>([]);
  readonly variantOptions = signal<{ label: string; value: number }[]>([]);
  readonly uomOptions = signal<{ label: string; value: number }[]>([]);

  form = this.fb.nonNullable.group({
    sourceWarehouseId: this.fb.control<number | null>(null, [Validators.required]),
    destinationWarehouseId: this.fb.control<number | null>(null, [Validators.required]),
    date: this.fb.control<Date | null>(new Date(), [Validators.required]),
    narration: this.fb.control<string | null>(null),
    lines: this.fb.array<LineGroup>([]),
  });

  readonly receiveRows = this.fb.array<ReceiveRow>([]);

  get linesArray(): FormArray<LineGroup> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    this.loadLookups();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.transferId.set(Number(idParam));
      this.loadTransfer(Number(idParam));
    } else {
      this.addLine();
    }
  }

  addLine(): void {
    this.linesArray.push(
      this.fb.group({
        productVariantId: this.fb.control<number | null>(null, [Validators.required]),
        unitOfMeasureId: this.fb.control<number | null>(null, [Validators.required]),
        qty: this.fb.nonNullable.control<number>(1),
      }),
    );
  }

  removeLine(index: number): void {
    if (this.linesArray.length > 1) {
      this.linesArray.removeAt(index);
    }
  }

  canSave(): boolean {
    return (
      !this.saving() &&
      this.form.controls.sourceWarehouseId.valid &&
      this.form.controls.destinationWarehouseId.valid &&
      this.form.controls.sourceWarehouseId.value !== this.form.controls.destinationWarehouseId.value &&
      this.linesArray.getRawValue().some((l) => l.productVariantId && l.qty > 0)
    );
  }

  submit(): void {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const lines: StockTransferLineRequest[] = this.linesArray
      .getRawValue()
      .filter((l) => l.productVariantId && l.qty > 0)
      .map((l) => ({ productVariantId: l.productVariantId!, unitOfMeasureId: l.unitOfMeasureId!, qty: l.qty }));

    const request: StockTransferRequest = {
      sourceWarehouseId: raw.sourceWarehouseId!,
      destinationWarehouseId: raw.destinationWarehouseId!,
      date: toIsoDate(raw.date),
      narration: raw.narration,
      lines,
    };

    this.saving.set(true);
    this.transferService.create(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success('Stock transfer created.');
        this.router.navigate(['/stock-transfers']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save.'));
      },
    });
  }

  cancelForm(): void {
    this.router.navigate(['/stock-transfers']);
  }

  submitForApproval(): void {
    const id = this.transferId();
    if (!id) return;
    this.transferService.submit(id).subscribe({
      next: () => {
        this.notificationService.success('Submitted for approval.');
        this.router.navigate(['/stock-transfers']);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(): void {
    const id = this.transferId();
    if (!id) return;
    this.transferService.approve(id).subscribe({
      next: () => {
        this.notificationService.success('Approved.');
        this.router.navigate(['/stock-transfers']);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(): void {
    const id = this.transferId();
    if (!id) return;
    this.transferService.reject(id).subscribe({
      next: () => {
        this.notificationService.success('Rejected.');
        this.router.navigate(['/stock-transfers']);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject.'));
      },
    });
  }

  confirmReceipt(): void {
    const id = this.transferId();
    if (!id) return;

    const lines: StockTransferReceiveLineRequest[] = this.receiveRows
      .getRawValue()
      .filter((r) => r.receiveNow > 0)
      .map((r) => ({ lineId: r.lineId, receivedBaseQty: r.receiveNow }));

    if (lines.length === 0) {
      this.notificationService.error('Enter a quantity to receive for at least one line.');
      return;
    }

    this.receiving.set(true);
    this.transferService.receive(id, { lines }).subscribe({
      next: () => {
        this.receiving.set(false);
        this.notificationService.success('Receipt recorded.');
        this.loadTransfer(id);
      },
      error: (error: HttpErrorResponse) => {
        this.receiving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to record receipt.'));
      },
    });
  }

  private loadLookups(): void {
    forkJoin({
      warehouses: this.warehouseService.getAll(),
      products: this.productService.getAll(),
      units: this.unitService.getAll(),
    }).subscribe({
      next: ({ warehouses, products, units }) => {
        this.warehouseOptions.set((warehouses.data ?? []).map((w) => ({ label: w.name, value: w.id })));
        this.uomOptions.set((units.data ?? []).map((u) => ({ label: `${u.code} - ${u.name}`, value: u.id })));

        const options: { label: string; value: number }[] = [];
        for (const product of products.data ?? []) {
          for (const variant of product.variants.filter((v) => v.isActive)) {
            options.push({ label: `${product.name} - ${variant.name} (${variant.variantCode})`, value: variant.id });
          }
        }
        this.variantOptions.set(options);
      },
    });
  }

  private loadTransfer(id: number): void {
    this.loading.set(true);
    this.transferService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        const transfer = response.data;
        if (!transfer) {
          this.notificationService.error('Not found.');
          this.router.navigate(['/stock-transfers']);
          return;
        }

        this.readOnly.set(true);
        this.current.set(transfer);
        this.form.patchValue({
          sourceWarehouseId: transfer.sourceWarehouseId,
          destinationWarehouseId: transfer.destinationWarehouseId,
          date: new Date(transfer.date),
          narration: transfer.narration,
        });

        this.linesArray.clear();
        for (const line of transfer.lines) {
          this.linesArray.push(
            this.fb.group({
              productVariantId: this.fb.control<number | null>(line.productVariantId),
              unitOfMeasureId: this.fb.control<number | null>(line.unitOfMeasureId),
              qty: this.fb.nonNullable.control<number>(line.qty),
            }),
          );
        }
        this.form.disable();

        this.receiveRows.clear();
        if (transfer.status === 'PendingReceipt') {
          for (const line of transfer.lines) {
            const remaining = line.baseQty - line.receivedBaseQty;
            if (remaining > 0) {
              this.receiveRows.push(
                this.fb.group({
                  lineId: this.fb.nonNullable.control(line.id),
                  label: this.fb.nonNullable.control(`${line.productName} - ${line.productVariantName}`),
                  remaining: this.fb.nonNullable.control(remaining),
                  receiveNow: this.fb.nonNullable.control(remaining),
                }),
              );
            }
          }
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load.'));
        this.router.navigate(['/stock-transfers']);
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
