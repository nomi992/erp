import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { ChipModule } from 'primeng/chip';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { PasswordModule } from 'primeng/password';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../../core/auth/auth.service';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { UserService } from '../../../core/users/user.service';
import { AdminUser, CreateUserRequest, UpdateUserRequest } from '../../../core/users/user.models';
import { BranchService } from '../../../core/branches/branch.service';
import { Branch } from '../../../core/branches/branch.models';
import { TenantService } from '../../../core/tenants/tenant.service';
import { Tenant } from '../../../core/tenants/tenant.models';
import { RoleService } from '../../../core/roles/role.service';
import { RoleSummary } from '../../../core/roles/role.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-users',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    ChipModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    MultiSelectModule,
    PasswordModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    ToggleSwitchModule,
    TooltipModule,
  ],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly branchService = inject(BranchService);
  private readonly tenantService = inject(TenantService);
  private readonly roleService = inject(RoleService);
  private readonly notificationService = inject(NotificationService);

  readonly isSystemAdmin = computed(() => this.authService.role() === 'SystemAdmin');

  readonly allRoles = signal<RoleSummary[]>([]);
  readonly roleOptions = computed(() =>
    this.allRoles()
      .filter((r) => this.isSystemAdmin() || r.name !== 'SystemAdmin')
      .map((r) => ({ label: r.name, value: r.id })),
  );

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly users = signal<AdminUser[]>([]);
  readonly tenants = signal<Tenant[]>([]);
  readonly tenantFilter = signal<number | null>(null);
  readonly tenantOptions = computed(() => this.tenants().map((t) => ({ label: t.name, value: t.id })));

  readonly createBranchOptions = signal<Branch[]>([]);
  readonly createDialogVisible = signal(false);

  readonly editDialogVisible = signal(false);
  readonly editingUser = signal<AdminUser | null>(null);

  readonly branchDialogVisible = signal(false);
  readonly managingUser = signal<AdminUser | null>(null);
  readonly grantableBranches = signal<Branch[]>([]);
  readonly selectedGrantBranchId = signal<number | null>(null);

  createForm = this.fb.nonNullable.group({
    tenantId: this.fb.control<number | null>(null),
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
    roleId: this.fb.control<number | null>(null, [Validators.required]),
    branchIds: this.fb.nonNullable.control<number[]>([]),
  });

  editForm = this.fb.nonNullable.group({
    roleId: this.fb.control<number | null>(null, [Validators.required]),
    isActive: [true],
  });

  ngOnInit(): void {
    this.roleService.getAll().subscribe({
      next: (response) => this.allRoles.set(response.data ?? []),
      error: () => this.notificationService.error('Unable to load roles.'),
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
    this.userService.getAll(this.tenantFilter()).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.users.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load users.'));
      },
    });
  }

  onTenantFilterChange(): void {
    this.load();
  }

  openCreateDialog(): void {
    this.createForm.reset({ tenantId: this.tenantFilter(), username: '', password: '', roleId: null, branchIds: [] });
    this.createBranchOptions.set([]);
    this.loadCreateBranchOptions(this.tenantFilter());
    this.createDialogVisible.set(true);
  }

  onCreateTenantChange(tenantId: number | null): void {
    this.createForm.controls.branchIds.setValue([]);
    this.loadCreateBranchOptions(tenantId);
  }

  private loadCreateBranchOptions(tenantId: number | null): void {
    if (this.isSystemAdmin() && tenantId == null) {
      this.createBranchOptions.set([]);
      return;
    }

    this.branchService.getAll(tenantId).subscribe({
      next: (response) => this.createBranchOptions.set(response.data ?? []),
      error: () => this.notificationService.error('Unable to load branches.'),
    });
  }

  closeCreateDialog(): void {
    this.createDialogVisible.set(false);
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.createForm.getRawValue();
    const request: CreateUserRequest = { ...raw, roleId: raw.roleId! };

    this.userService.create(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success('User created successfully.');
        this.createDialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to create user.'));
      },
    });
  }

  openEditDialog(user: AdminUser): void {
    this.editingUser.set(user);
    this.editForm.reset({ roleId: user.roleId, isActive: user.isActive });
    this.editDialogVisible.set(true);
  }

  closeEditDialog(): void {
    this.editDialogVisible.set(false);
  }

  submitEdit(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const editing = this.editingUser();
    if (!editing) {
      return;
    }

    this.saving.set(true);
    const raw = this.editForm.getRawValue();
    const request: UpdateUserRequest = { ...raw, roleId: raw.roleId! };

    this.userService.update(editing.id, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success('User updated successfully.');
        this.editDialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update user.'));
      },
    });
  }

  openBranchDialog(user: AdminUser): void {
    this.managingUser.set(user);
    this.selectedGrantBranchId.set(null);
    this.branchService.getAll(user.tenantId).subscribe({
      next: (response) => {
        const grantedIds = new Set(user.branches.map((b) => b.id));
        this.grantableBranches.set((response.data ?? []).filter((b) => !grantedIds.has(b.id)));
      },
      error: () => this.notificationService.error('Unable to load branches.'),
    });
    this.branchDialogVisible.set(true);
  }

  closeBranchDialog(): void {
    this.branchDialogVisible.set(false);
  }

  grantBranch(): void {
    const user = this.managingUser();
    const branchId = this.selectedGrantBranchId();
    if (!user || branchId == null) {
      return;
    }

    this.userService.grantBranch(user.id, branchId).subscribe({
      next: () => {
        this.notificationService.success('Branch access granted.');
        this.load();
        this.openBranchDialog({ ...user, branches: [...user.branches, { id: branchId, name: '', code: '' }] });
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to grant branch access.'));
      },
    });
  }

  revokeBranch(branchId: number): void {
    const user = this.managingUser();
    if (!user) {
      return;
    }

    this.userService.revokeBranch(user.id, branchId).subscribe({
      next: () => {
        this.notificationService.success('Branch access revoked.');
        this.load();
        this.openBranchDialog({ ...user, branches: user.branches.filter((b) => b.id !== branchId) });
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to revoke branch access.'));
      },
    });
  }

  toggleActive(user: AdminUser): void {
    const call = user.isActive ? this.userService.deactivate(user.id) : this.userService.activate(user.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(user.isActive ? 'User deactivated.' : 'User activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update user status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
