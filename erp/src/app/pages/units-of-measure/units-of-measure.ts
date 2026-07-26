import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { UnitOfMeasureService } from '../../core/units-of-measure/unit-of-measure.service';
import { UnitOfMeasure, UnitOfMeasureRequest } from '../../core/units-of-measure/unit-of-measure.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-units-of-measure',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './units-of-measure.html',
  styleUrl: './units-of-measure.scss',
})
export class UnitsOfMeasure implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly unitService = inject(UnitOfMeasureService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly units = signal<UnitOfMeasure[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingUnit = signal<UnitOfMeasure | null>(null);

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required]],
    name: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.unitService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.units.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load units of measure.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingUnit.set(null);
    this.form.reset({ code: '', name: '' });
    this.dialogVisible.set(true);
  }

  openEditDialog(unit: UnitOfMeasure): void {
    this.editingUnit.set(unit);
    this.form.reset({ code: unit.code, name: unit.name });
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
    const request: UnitOfMeasureRequest = this.form.getRawValue();
    const editing = this.editingUnit();

    const call = editing ? this.unitService.update(editing.id, request) : this.unitService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Unit of measure updated successfully.' : 'Unit of measure created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save unit of measure.'));
      },
    });
  }

  toggleActive(unit: UnitOfMeasure): void {
    const call = unit.isActive ? this.unitService.deactivate(unit.id) : this.unitService.activate(unit.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(unit.isActive ? 'Unit of measure deactivated.' : 'Unit of measure activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update unit of measure status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
