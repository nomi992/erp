import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { FieldsetModule } from 'primeng/fieldset';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../../core/auth/auth.service';
import { RoleService } from '../../../core/roles/role.service';
import { RoleRequest, RoleResponse } from '../../../core/roles/role.models';
import { RightsService } from '../../../core/rights/rights.service';
import { RightGroup } from '../../../core/rights/right.models';
import { TenantService } from '../../../core/tenants/tenant.service';
import { Tenant } from '../../../core/tenants/tenant.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-roles',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    ConfirmDialogModule,
    DialogModule,
    FieldsetModule,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './roles.html',
  styleUrl: './roles.scss',
})
export class Roles implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly roleService = inject(RoleService);
  private readonly rightsService = inject(RightsService);
  private readonly tenantService = inject(TenantService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly isSystemAdmin = computed(() => this.authService.role() === 'SystemAdmin');

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly roles = signal<RoleResponse[]>([]);
  readonly rightGroups = signal<RightGroup[]>([]);
  readonly tenants = signal<Tenant[]>([]);
  readonly tenantFilter = signal<number | null>(null);
  readonly tenantOptions = computed(() => this.tenants().map((t) => ({ label: t.name, value: t.id })));

  readonly dialogVisible = signal(false);
  readonly editingRole = signal<RoleResponse | null>(null);
  readonly selectedRightIds = signal<Set<number>>(new Set());

  form = this.fb.nonNullable.group({
    tenantId: this.fb.control<number | null>(null),
    name: ['', [Validators.required]],
    description: [''],
  });

  ngOnInit(): void {
    this.rightsService.getAll().subscribe({
      next: (response) => this.rightGroups.set(response.data ?? []),
      error: () => this.notificationService.error('Unable to load rights.'),
    });

    if (this.isSystemAdmin()) {
      this.tenantService.getAll().subscribe({
        next: (response) => this.tenants.set(response.data ?? []),
        error: () => this.notificationService.error('Unable to load tenants.'),
      });
    }

    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.roleService.getAll(this.tenantFilter()).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.roles.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load roles.'));
      },
    });
  }

  onTenantFilterChange(): void {
    this.load();
  }

  isRightSelected(rightId: number): boolean {
    return this.selectedRightIds().has(rightId);
  }

  toggleRight(rightId: number, checked: boolean): void {
    const next = new Set(this.selectedRightIds());
    if (checked) {
      next.add(rightId);
    } else {
      next.delete(rightId);
    }
    this.selectedRightIds.set(next);
  }

  openCreateDialog(): void {
    this.editingRole.set(null);
    this.form.reset({ tenantId: this.tenantFilter(), name: '', description: '' });
    this.form.controls.name.enable();
    this.form.controls.tenantId.enable();
    this.selectedRightIds.set(new Set());
    this.dialogVisible.set(true);
  }

  openEditDialog(role: RoleResponse): void {
    this.editingRole.set(role);
    this.form.reset({ tenantId: role.tenantId, name: role.name, description: role.description ?? '' });
    role.isSystemRole ? this.form.controls.name.disable() : this.form.controls.name.enable();
    this.form.controls.tenantId.disable();
    this.selectedRightIds.set(new Set(role.rights.map((r) => r.id)));
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
    const editing = this.editingRole();
    const { tenantId, name, description } = this.form.getRawValue();
    const request: RoleRequest = {
      tenantId,
      name,
      description: description || null,
      rightIds: [...this.selectedRightIds()],
    };

    const call = editing ? this.roleService.update(editing.id, request) : this.roleService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Role updated successfully.' : 'Role created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save role.'));
      },
    });
  }

  deleteRole(role: RoleResponse): void {
    if (role.isSystemRole) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete Role',
      message: `Delete the "${role.name}" role? Users assigned to it must be reassigned first.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger', label: 'Delete' },
      rejectButtonProps: { severity: 'secondary', outlined: true, label: 'Cancel' },
      accept: () => {
        this.roleService.delete(role.id).subscribe({
          next: () => {
            this.notificationService.success('Role deleted successfully.');
            this.load();
          },
          error: (error: HttpErrorResponse) => {
            this.notificationService.error(this.extractErrorMessage(error, 'Unable to delete role.'));
          },
        });
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
