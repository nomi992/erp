import AsyncStorage from '@react-native-async-storage/async-storage';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { registerBranchAccessor } from '../api/client';
import type { LoginResponse } from '../auth/auth.types';
import type { BranchSummary } from './tenancy.types';

const STORAGE_KEY = 'erp.tenancy';

interface StoredTenancy {
  tenantId: number;
  tenantName: string;
  branches: BranchSummary[];
  currentBranchId: number | null;
}

interface TenancyContextValue {
  tenantName: string | null;
  branches: BranchSummary[];
  currentBranchId: number | null;
  currentBranch: BranchSummary | null;
  hasBranches: boolean;
  /** Called by AuthProvider right after a successful login. */
  setFromLogin: (data: LoginResponse) => Promise<void>;
  selectBranch: (branchId: number) => Promise<void>;
  /** Called by AuthProvider on logout. */
  clear: () => Promise<void>;
}

const TenancyContext = createContext<TenancyContextValue | null>(null);

// Mirrors erp/src/app/core/tenancy/tenancy.service.ts. Unlike the web app, there is no
// SystemAdmin cross-tenant "override" support here — that's a niche admin feature we
// deliberately left out of the mobile app's scope.
export function TenancyProvider({ children }: { children: React.ReactNode }) {
  const [tenancy, setTenancyState] = useState<StoredTenancy | null>(null);

  useEffect(() => {
    (async () => {
      const raw = await AsyncStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return;
      }
      try {
        setTenancyState(JSON.parse(raw) as StoredTenancy);
      } catch {
        await AsyncStorage.removeItem(STORAGE_KEY);
      }
    })();
  }, []);

  useEffect(() => {
    registerBranchAccessor(() => tenancy?.currentBranchId ?? null);
  }, [tenancy]);

  const setFromLogin = useCallback(async (data: LoginResponse) => {
    // Same default as web: auto-select the first branch, let the user switch afterwards.
    const stored: StoredTenancy = {
      tenantId: data.tenantId,
      tenantName: data.tenantName,
      branches: data.branches,
      currentBranchId: data.branches[0]?.id ?? null,
    };
    await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    setTenancyState(stored);
  }, []);

  const selectBranch = useCallback(async (branchId: number) => {
    setTenancyState((current) => {
      if (!current) {
        return current;
      }
      const updated: StoredTenancy = { ...current, currentBranchId: branchId };
      void AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
      return updated;
    });
  }, []);

  const clear = useCallback(async () => {
    await AsyncStorage.removeItem(STORAGE_KEY);
    setTenancyState(null);
  }, []);

  const branches = tenancy?.branches ?? [];
  const currentBranchId = tenancy?.currentBranchId ?? null;

  const value = useMemo<TenancyContextValue>(
    () => ({
      tenantName: tenancy?.tenantName ?? null,
      branches,
      currentBranchId,
      currentBranch: branches.find((b) => b.id === currentBranchId) ?? null,
      hasBranches: branches.length > 0,
      setFromLogin,
      selectBranch,
      clear,
    }),
    [tenancy, branches, currentBranchId, setFromLogin, selectBranch, clear],
  );

  return <TenancyContext.Provider value={value}>{children}</TenancyContext.Provider>;
}

export function useTenancy(): TenancyContextValue {
  const ctx = useContext(TenancyContext);
  if (!ctx) {
    throw new Error('useTenancy must be used within a TenancyProvider');
  }
  return ctx;
}
