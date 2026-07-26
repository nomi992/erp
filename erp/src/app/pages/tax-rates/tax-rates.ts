import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { TaxRateService } from '../../core/tax-rates/tax-rate.service';
import { TaxRate, TaxRateRequest } from '../../core/tax-rates/tax-rate.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-tax-rates',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    HasRightDirective,
    InputNumberModule,
    InputTextModule,
    PrimeTemplate,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './tax-rates.html',
  styleUrl: './tax-rates.scss',
})
export class TaxRates implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly taxRateService = inject(TaxRateService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly taxRates = signal<TaxRate[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingTaxRate = signal<TaxRate | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    percentage: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.taxRateService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.taxRates.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load tax rates.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingTaxRate.set(null);
    this.form.reset({ name: '', percentage: 0 });
    this.dialogVisible.set(true);
  }

  openEditDialog(taxRate: TaxRate): void {
    this.editingTaxRate.set(taxRate);
    this.form.reset({ name: taxRate.name, percentage: taxRate.percentage });
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
    const request: TaxRateRequest = this.form.getRawValue();
    const editing = this.editingTaxRate();

    const call = editing ? this.taxRateService.update(editing.id, request) : this.taxRateService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Tax rate updated successfully.' : 'Tax rate created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save tax rate.'));
      },
    });
  }

  toggleActive(taxRate: TaxRate): void {
    const call = taxRate.isActive ? this.taxRateService.deactivate(taxRate.id) : this.taxRateService.activate(taxRate.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(taxRate.isActive ? 'Tax rate deactivated.' : 'Tax rate activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update tax rate status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
