import { Injectable, computed, signal } from '@angular/core';
import { LoginResponse } from '../auth/auth.models';
import { BranchSummary } from './tenancy.models';

const STORAGE_KEY = 'erp.tenancy';
const OVERRIDE_STORAGE_KEY = 'erp.tenancy.override';

interface StoredTenancy {
  tenantId: number;
  tenantName: string;
  branches: BranchSummary[];
  currentBranchId: number | null;
}

// SystemAdmin-only: the tenant/branch they're currently browsing, distinct from their own
// identity above (a SystemAdmin belongs to no real tenant and has no branch grants of their own).
interface StoredOverride {
  tenantId: number;
  tenantName: string;
  branchId: number | null;
  branchName: string | null;
}

function readStoredTenancy(): StoredTenancy | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as StoredTenancy;
  } catch {
    return null;
  }
}

function readStoredOverride(): StoredOverride | null {
  const raw = localStorage.getItem(OVERRIDE_STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as StoredOverride;
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class TenancyService {
  private readonly tenancy = signal<StoredTenancy | null>(readStoredTenancy());
  private readonly override = signal<StoredOverride | null>(readStoredOverride());

  readonly tenantId = computed(() => this.tenancy()?.tenantId ?? null);
  readonly tenantName = computed(() => this.tenancy()?.tenantName ?? null);
  readonly branches = computed(() => this.tenancy()?.branches ?? []);
  readonly currentBranchId = computed(() => this.tenancy()?.currentBranchId ?? null);
  readonly currentBranch = computed(
    () => this.branches().find((b) => b.id === this.currentBranchId()) ?? null,
  );
  readonly hasBranches = computed(() => this.branches().length > 0);

  readonly overrideTenantId = computed(() => this.override()?.tenantId ?? null);
  readonly overrideTenantName = computed(() => this.override()?.tenantName ?? null);
  readonly overrideBranchId = computed(() => this.override()?.branchId ?? null);
  readonly overrideBranchName = computed(() => this.override()?.branchName ?? null);
  readonly isOverriding = computed(() => this.override() !== null);

  readonly effectiveTenantName = computed(() => this.overrideTenantName() ?? this.tenantName());

  setFromLogin(data: LoginResponse): void {
    const stored: StoredTenancy = {
      tenantId: data.tenantId,
      tenantName: data.tenantName,
      branches: data.branches,
      currentBranchId: data.branches[0]?.id ?? null,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    this.tenancy.set(stored);
  }

  selectBranch(branchId: number): void {
    const current = this.tenancy();
    if (!current) {
      return;
    }

    const updated: StoredTenancy = { ...current, currentBranchId: branchId };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    this.tenancy.set(updated);
    window.location.reload();
  }

  setOverrideTenant(tenantId: number, tenantName: string): void {
    const updated: StoredOverride = { tenantId, tenantName, branchId: null, branchName: null };
    localStorage.setItem(OVERRIDE_STORAGE_KEY, JSON.stringify(updated));
    this.override.set(updated);
    window.location.reload();
  }

  setOverrideBranch(branchId: number | null, branchName: string | null): void {
    const current = this.override();
    if (!current) {
      return;
    }

    const updated: StoredOverride = { ...current, branchId, branchName };
    localStorage.setItem(OVERRIDE_STORAGE_KEY, JSON.stringify(updated));
    this.override.set(updated);
    window.location.reload();
  }

  clearOverride(): void {
    localStorage.removeItem(OVERRIDE_STORAGE_KEY);
    this.override.set(null);
    window.location.reload();
  }

  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem(OVERRIDE_STORAGE_KEY);
    this.tenancy.set(null);
    this.override.set(null);
  }
}
