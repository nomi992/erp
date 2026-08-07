import { Component, OnInit, computed, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { SelectModule } from 'primeng/select';
import { AuthService } from '../../core/auth/auth.service';
import { TenancyService } from '../../core/tenancy/tenancy.service';
import { TenantService } from '../../core/tenants/tenant.service';
import { BranchService } from '../../core/branches/branch.service';
import { Branch } from '../../core/branches/branch.models';

interface TenantOption {
  label: string;
  value: number;
}

@Component({
  selector: 'app-topbar',
  imports: [FormsModule, ToolbarModule, ButtonModule, AvatarModule, SelectModule],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss',
})
export class Topbar implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly branchService = inject(BranchService);
  readonly tenancyService = inject(TenancyService);

  readonly toggleSidebar = output<void>();

  readonly isSystemAdmin = computed(() => this.authService.role() === 'SystemAdmin');
  readonly tenantOptions = signal<TenantOption[]>([]);
  readonly overrideBranchOptions = signal<Branch[]>([]);

  ngOnInit(): void {
    if (!this.isSystemAdmin()) {
      return;
    }

    this.tenantService.getAll().subscribe({
      next: (response) => {
        this.tenantOptions.set((response.data ?? []).map((t) => ({ label: t.name, value: t.id })));
      },
    });

    const overrideTenantId = this.tenancyService.overrideTenantId();
    if (overrideTenantId != null) {
      this.loadOverrideBranches(overrideTenantId);
    }
  }

  onBranchChange(branchId: number): void {
    this.tenancyService.selectBranch(branchId);
  }

  onOverrideTenantChange(tenantId: number): void {
    const tenant = this.tenantOptions().find((t) => t.value === tenantId);
    if (!tenant) {
      return;
    }

    this.tenancyService.setOverrideTenant(tenantId, tenant.label);
  }

  onOverrideBranchChange(branchId: number | null): void {
    const branch = branchId != null ? this.overrideBranchOptions().find((b) => b.id === branchId) : null;
    this.tenancyService.setOverrideBranch(branchId, branch?.name ?? null);
  }

  clearOverride(): void {
    this.tenancyService.clearOverride();
  }

  logout(): void {
    this.authService.logout();
  }

  private loadOverrideBranches(tenantId: number): void {
    this.branchService.getAll(tenantId).subscribe({
      next: (response) => this.overrideBranchOptions.set(response.data ?? []),
    });
  }
}
