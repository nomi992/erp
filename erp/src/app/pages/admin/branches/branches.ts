import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../../core/auth/auth.service';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { BranchService } from '../../../core/branches/branch.service';
import { Branch, BranchRequest } from '../../../core/branches/branch.models';
import { TenantService } from '../../../core/tenants/tenant.service';
import { Tenant } from '../../../core/tenants/tenant.models';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-branches',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './branches.html',
  styleUrl: './branches.scss',
})
export class Branches implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly branchService = inject(BranchService);
  private readonly tenantService = inject(TenantService);
  private readonly notificationService = inject(NotificationService);

  readonly isSystemAdmin = computed(() => this.authService.role() === 'SystemAdmin');

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly branches = signal<Branch[]>([]);
  readonly tenants = signal<Tenant[]>([]);
  readonly tenantFilter = signal<number | null>(null);
  readonly dialogVisible = signal(false);
  readonly editingBranch = signal<Branch | null>(null);

  readonly tenantOptions = computed(() => this.tenants().map((t) => ({ label: t.name, value: t.id })));

  form = this.fb.nonNullable.group({
    tenantId: this.fb.control<number | null>(null),
    name: ['', [Validators.required]],
    code: ['', [Validators.required]],
  });

  ngOnInit(): void {
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
    this.branchService.getAll(this.tenantFilter()).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.branches.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load branches.'));
      },
    });
  }

  onTenantFilterChange(): void {
    this.load();
  }

  openCreateDialog(): void {
    this.editingBranch.set(null);
    this.form.reset({ tenantId: this.tenantFilter(), name: '', code: '' });
    this.dialogVisible.set(true);
  }

  openEditDialog(branch: Branch): void {
    this.editingBranch.set(branch);
    this.form.reset({ tenantId: branch.tenantId, name: branch.name, code: branch.code });
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
    const editing = this.editingBranch();
    const request: BranchRequest = this.form.getRawValue();

    const call = editing ? this.branchService.update(editing.id, request) : this.branchService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Branch updated successfully.' : 'Branch created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save branch.'));
      },
    });
  }

  toggleActive(branch: Branch): void {
    const call = branch.isActive ? this.branchService.deactivate(branch.id) : this.branchService.activate(branch.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(branch.isActive ? 'Branch deactivated.' : 'Branch activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update branch status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
