import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { CostCenterService } from '../../core/cost-centers/cost-center.service';
import { CostCenter, CostCenterRequest } from '../../core/cost-centers/cost-center.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-cost-centers',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './cost-centers.html',
  styleUrl: './cost-centers.scss',
})
export class CostCenters implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly costCenterService = inject(CostCenterService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly costCenters = signal<CostCenter[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingCostCenter = signal<CostCenter | null>(null);

  readonly parentOptions = signal<{ label: string; value: number | null }[]>([]);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    parentCostCenterId: this.fb.control<number | null>(null),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.costCenterService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.costCenters.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load cost centers.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingCostCenter.set(null);
    this.form.reset({ name: '', parentCostCenterId: null });
    this.setParentOptions(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(costCenter: CostCenter): void {
    this.editingCostCenter.set(costCenter);
    this.form.reset({
      name: costCenter.name,
      parentCostCenterId: costCenter.parentCostCenterId,
    });
    this.setParentOptions(costCenter);
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
    const request: CostCenterRequest = this.form.getRawValue();
    const editing = this.editingCostCenter();

    const call = editing
      ? this.costCenterService.update(editing.id, request)
      : this.costCenterService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(
          editing ? 'Cost center updated successfully.' : 'Cost center created successfully.',
        );
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save cost center.'));
      },
    });
  }

  toggleActive(costCenter: CostCenter): void {
    const call = costCenter.isActive
      ? this.costCenterService.deactivate(costCenter.id)
      : this.costCenterService.activate(costCenter.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(costCenter.isActive ? 'Cost center deactivated.' : 'Cost center activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update cost center status.'));
      },
    });
  }

  confirmDelete(costCenter: CostCenter): void {
    this.confirmationService.confirm({
      header: 'Delete Cost Center',
      message: `Delete cost center "${costCenter.name}"? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Delete' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => this.deleteCostCenter(costCenter),
    });
  }

  private deleteCostCenter(costCenter: CostCenter): void {
    this.costCenterService.delete(costCenter.id).subscribe({
      next: () => {
        this.notificationService.success('Cost center deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to delete cost center.'));
      },
    });
  }

  private setParentOptions(editing: CostCenter | null): void {
    const options = this.costCenters()
      .filter((c) => c.id !== editing?.id)
      .map((c) => ({ label: c.name, value: c.id }));

    this.parentOptions.set([{ label: '(None — root cost center)', value: null }, ...options]);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
