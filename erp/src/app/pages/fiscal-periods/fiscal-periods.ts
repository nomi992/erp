import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { FiscalPeriodService } from '../../core/fiscal-periods/fiscal-period.service';
import { FiscalPeriod, FiscalPeriodRequest } from '../../core/fiscal-periods/fiscal-period.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-fiscal-periods',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    ButtonModule,
    CardModule,
    DatePickerModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './fiscal-periods.html',
  styleUrl: './fiscal-periods.scss',
})
export class FiscalPeriods implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly fiscalPeriodService = inject(FiscalPeriodService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly fiscalPeriods = signal<FiscalPeriod[]>([]);
  readonly dialogVisible = signal(false);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    startDate: this.fb.control<Date | null>(null, [Validators.required]),
    endDate: this.fb.control<Date | null>(null, [Validators.required]),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.fiscalPeriodService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.fiscalPeriods.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load fiscal periods.'));
      },
    });
  }

  openCreateDialog(): void {
    this.form.reset({ name: '', startDate: null, endDate: null });
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

    const { name, startDate, endDate } = this.form.getRawValue();

    if (startDate && endDate && endDate < startDate) {
      this.notificationService.error('End date must be on or after the start date.');
      return;
    }

    this.saving.set(true);
    const request: FiscalPeriodRequest = {
      name,
      startDate: formatDate(startDate as Date, 'yyyy-MM-dd', 'en-US'),
      endDate: formatDate(endDate as Date, 'yyyy-MM-dd', 'en-US'),
    };

    this.fiscalPeriodService.create(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success('Fiscal period created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to create fiscal period.'));
      },
    });
  }

  toggleClosed(fiscalPeriod: FiscalPeriod): void {
    const call = fiscalPeriod.isClosed
      ? this.fiscalPeriodService.open(fiscalPeriod.id)
      : this.fiscalPeriodService.close(fiscalPeriod.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(fiscalPeriod.isClosed ? 'Fiscal period reopened.' : 'Fiscal period closed.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update fiscal period status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
