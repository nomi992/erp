import { Injectable, computed, signal } from '@angular/core';
import { LoginResponse } from '../auth/auth.models';
import { BranchSummary } from './tenancy.models';

const STORAGE_KEY = 'erp.tenancy';

interface StoredTenancy {
  tenantId: number;
  tenantName: string;
  branches: BranchSummary[];
  currentBranchId: number | null;
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

@Injectable({ providedIn: 'root' })
export class TenancyService {
  private readonly tenancy = signal<StoredTenancy | null>(readStoredTenancy());

  readonly tenantId = computed(() => this.tenancy()?.tenantId ?? null);
  readonly tenantName = computed(() => this.tenancy()?.tenantName ?? null);
  readonly branches = computed(() => this.tenancy()?.branches ?? []);
  readonly currentBranchId = computed(() => this.tenancy()?.currentBranchId ?? null);
  readonly currentBranch = computed(
    () => this.branches().find((b) => b.id === this.currentBranchId()) ?? null,
  );
  readonly hasBranches = computed(() => this.branches().length > 0);

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

  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.tenancy.set(null);
  }
}
