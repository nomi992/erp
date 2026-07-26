import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { TenantService } from '../../../core/tenants/tenant.service';
import { Tenant, CreateTenantRequest, UpdateTenantRequest } from '../../../core/tenants/tenant.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-tenants',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    ConfirmDialogModule,
    DialogModule,
    InputTextModule,
    PrimeTemplate,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './tenants.html',
  styleUrl: './tenants.scss',
})
export class Tenants implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly tenantService = inject(TenantService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly tenants = signal<Tenant[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingTenant = signal<Tenant | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    code: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.tenantService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.tenants.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load tenants.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingTenant.set(null);
    this.form.reset({ name: '', code: '' });
    this.form.controls.code.enable();
    this.dialogVisible.set(true);
  }

  openEditDialog(tenant: Tenant): void {
    this.editingTenant.set(tenant);
    this.form.reset({ name: tenant.name, code: tenant.code });
    this.form.controls.code.disable();
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
    const editing = this.editingTenant();
    const { name, code } = this.form.getRawValue();

    const call = editing
      ? this.tenantService.update(editing.id, { name } satisfies UpdateTenantRequest)
      : this.tenantService.create({ name, code } satisfies CreateTenantRequest);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Tenant updated successfully.' : 'Tenant created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save tenant.'));
      },
    });
  }

  toggleActive(tenant: Tenant): void {
    if (tenant.isActive) {
      this.confirmationService.confirm({
        header: 'Deactivate Tenant',
        message: `Deactivate "${tenant.name}"? All of its users will be unable to log in until reactivated.`,
        icon: 'pi pi-exclamation-triangle',
        acceptButtonProps: { severity: 'danger', label: 'Deactivate' },
        rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
        accept: () => this.setActive(tenant, false),
      });
      return;
    }

    this.setActive(tenant, true);
  }

  private setActive(tenant: Tenant, activate: boolean): void {
    const call = activate ? this.tenantService.activate(tenant.id) : this.tenantService.deactivate(tenant.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(activate ? 'Tenant activated.' : 'Tenant deactivated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update tenant status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
