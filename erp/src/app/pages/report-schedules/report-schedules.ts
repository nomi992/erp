import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
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
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { ReportSchedule, ReportScheduleRequest } from '../../core/reports/report-schedule.models';
import { ReportScheduleService } from '../../core/reports/report-schedule.service';
import { RecurringFrequency } from '../../core/vouchers/voucher.models';

const REPORT_TYPE_OPTIONS = [
  { label: 'Trial Balance', value: 'trial-balance' },
  { label: 'Profit & Loss', value: 'profit-loss' },
  { label: 'Balance Sheet', value: 'balance-sheet' },
  { label: 'Cash Flow', value: 'cash-flow' },
  { label: 'Aging', value: 'aging' },
  { label: 'Day Book', value: 'day-book' },
  { label: 'Budget vs Actual', value: 'budget-vs-actual' },
];

const FREQUENCY_OPTIONS: { label: string; value: RecurringFrequency }[] = [
  { label: 'Daily', value: 'Daily' },
  { label: 'Weekly', value: 'Weekly' },
  { label: 'Monthly', value: 'Monthly' },
  { label: 'Yearly', value: 'Yearly' },
];

@Component({
  selector: 'app-report-schedules',
  imports: [
    ReactiveFormsModule,
    DatePipe,
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
  templateUrl: './report-schedules.html',
  styleUrl: './report-schedules.scss',
})
export class ReportSchedules implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly reportScheduleService = inject(ReportScheduleService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly runningId = signal<number | null>(null);
  readonly schedules = signal<ReportSchedule[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingSchedule = signal<ReportSchedule | null>(null);

  readonly reportTypeOptions = REPORT_TYPE_OPTIONS;
  readonly frequencyOptions = FREQUENCY_OPTIONS;

  form = this.fb.nonNullable.group({
    reportType: ['trial-balance', [Validators.required]],
    recipients: ['', [Validators.required]],
    frequency: this.fb.nonNullable.control<RecurringFrequency>('Monthly', [Validators.required]),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.reportScheduleService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.schedules.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load report schedules.'));
      },
    });
  }

  reportTypeLabel(reportType: string): string {
    return this.reportTypeOptions.find((o) => o.value === reportType)?.label ?? reportType;
  }

  openCreateDialog(): void {
    this.editingSchedule.set(null);
    this.form.reset({ reportType: 'trial-balance', recipients: '', frequency: 'Monthly' });
    this.dialogVisible.set(true);
  }

  openEditDialog(schedule: ReportSchedule): void {
    this.editingSchedule.set(schedule);
    this.form.reset({
      reportType: schedule.reportType,
      recipients: schedule.recipients,
      frequency: schedule.frequency,
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
    const request: ReportScheduleRequest = this.form.getRawValue();
    const editing = this.editingSchedule();

    const call = editing
      ? this.reportScheduleService.update(editing.id, request)
      : this.reportScheduleService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Schedule updated successfully.' : 'Schedule created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save schedule.'));
      },
    });
  }

  toggleActive(schedule: ReportSchedule): void {
    const call = schedule.isActive
      ? this.reportScheduleService.deactivate(schedule.id)
      : this.reportScheduleService.activate(schedule.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(schedule.isActive ? 'Schedule deactivated.' : 'Schedule activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update schedule status.'));
      },
    });
  }

  runNow(schedule: ReportSchedule): void {
    this.runningId.set(schedule.id);
    this.reportScheduleService.runNow(schedule.id).subscribe({
      next: (blob) => {
        this.runningId.set(null);
        this.downloadBlob(blob, `${schedule.reportType}.pdf`);
        this.notificationService.success('Report generated and downloaded.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.runningId.set(null);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to generate report.'));
      },
    });
  }

  confirmDelete(schedule: ReportSchedule): void {
    this.confirmationService.confirm({
      header: 'Delete Schedule',
      message: `Delete the ${this.reportTypeLabel(schedule.reportType)} schedule for "${schedule.recipients}"? This cannot be undone.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Delete' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => this.deleteSchedule(schedule),
    });
  }

  private deleteSchedule(schedule: ReportSchedule): void {
    this.reportScheduleService.delete(schedule.id).subscribe({
      next: () => {
        this.notificationService.success('Schedule deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to delete schedule.'));
      },
    });
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
