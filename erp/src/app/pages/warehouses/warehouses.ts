import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { WarehouseService } from '../../core/warehouses/warehouse.service';
import { Warehouse, WarehouseRequest } from '../../core/warehouses/warehouse.models';
import { CostCenterService } from '../../core/cost-centers/cost-center.service';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-warehouses',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './warehouses.html',
  styleUrl: './warehouses.scss',
})
export class Warehouses implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly warehouseService = inject(WarehouseService);
  private readonly costCenterService = inject(CostCenterService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly warehouses = signal<Warehouse[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingWarehouse = signal<Warehouse | null>(null);
  readonly costCenterOptions = signal<{ label: string; value: number | null }[]>([{ label: '(None)', value: null }]);

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required]],
    name: ['', [Validators.required]],
    costCenterId: this.fb.control<number | null>(null),
    isDefault: this.fb.nonNullable.control<boolean>(false),
  });

  ngOnInit(): void {
    this.load();
    this.costCenterService.getAll().subscribe({
      next: (response) => {
        this.costCenterOptions.set([
          { label: '(None)', value: null },
          ...(response.data ?? []).map((cc) => ({ label: cc.name, value: cc.id as number | null })),
        ]);
      },
    });
  }

  load(): void {
    this.loading.set(true);
    this.warehouseService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.warehouses.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load warehouses.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingWarehouse.set(null);
    this.form.reset({ code: '', name: '', costCenterId: null, isDefault: false });
    this.dialogVisible.set(true);
  }

  openEditDialog(warehouse: Warehouse): void {
    this.editingWarehouse.set(warehouse);
    this.form.reset({
      code: warehouse.code,
      name: warehouse.name,
      costCenterId: warehouse.costCenterId,
      isDefault: warehouse.isDefault,
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
    const request: WarehouseRequest = this.form.getRawValue();
    const editing = this.editingWarehouse();

    const call = editing ? this.warehouseService.update(editing.id, request) : this.warehouseService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Warehouse updated successfully.' : 'Warehouse created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save warehouse.'));
      },
    });
  }

  toggleActive(warehouse: Warehouse): void {
    const call = warehouse.isActive ? this.warehouseService.deactivate(warehouse.id) : this.warehouseService.activate(warehouse.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(warehouse.isActive ? 'Warehouse deactivated.' : 'Warehouse activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update warehouse status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
