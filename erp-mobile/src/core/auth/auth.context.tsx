import AsyncStorage from '@react-native-async-storage/async-storage';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { apiClient, registerAuthAccessor, registerUnauthorizedHandler } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import { useTenancy } from '../tenancy/tenancy.context';
import type { LoginRequest, LoginResponse, StoredAuth } from './auth.types';

const STORAGE_KEY = 'erp.auth';

interface AuthContextValue {
  /** True once the AsyncStorage hydration on app start has finished. */
  isReady: boolean;
  isAuthenticated: boolean;
  username: string | null;
  role: string | null;
  rights: string[];
  hasRight: (code: string) => boolean;
  login: (request: LoginRequest) => Promise<LoginResponse>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

// Mirrors erp/src/app/core/auth/auth.service.ts: private state (here, React state backed by
// AsyncStorage instead of a localStorage-backed signal), exposed read-only, mutated only
// through this provider's own login()/logout(). Must be rendered inside a TenancyProvider —
// same dependency direction as the web AuthService injecting TenancyService.
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const tenancy = useTenancy();
  const [auth, setAuthState] = useState<StoredAuth | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    (async () => {
      const raw = await AsyncStorage.getItem(STORAGE_KEY);
      if (raw) {
        try {
          setAuthState(JSON.parse(raw) as StoredAuth);
        } catch {
          await AsyncStorage.removeItem(STORAGE_KEY);
        }
      }
      setIsReady(true);
    })();
  }, []);

  useEffect(() => {
    registerAuthAccessor(() => auth?.token ?? null);
  }, [auth]);

  const logout = useCallback(async () => {
    await AsyncStorage.removeItem(STORAGE_KEY);
    setAuthState(null);
    await tenancy.clear();
  }, [tenancy]);

  useEffect(() => {
    // The axios interceptor can't await a React state update, so fire-and-forget here —
    // same "force logout on 401" behavior as erp/src/app/core/auth/auth.interceptor.ts.
    registerUnauthorizedHandler(() => {
      void logout();
    });
  }, [logout]);

  const login = useCallback(
    async (request: LoginRequest): Promise<LoginResponse> => {
      const response = await apiClient.post<ApiResponse<LoginResponse>>('/api/auth/login', request);
      const data = response.data.data;
      if (!data) {
        throw new Error(response.data.message || 'Login failed.');
      }

      const stored: StoredAuth = {
        token: data.token,
        username: data.username,
        role: data.role,
        expiresAtUtc: data.expiresAtUtc,
        rights: data.rights,
      };
      await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
      setAuthState(stored);
      await tenancy.setFromLogin(data);

      return data;
    },
    [tenancy],
  );

  const hasRight = useCallback((code: string) => auth?.rights.includes(code) ?? false, [auth]);

  const value = useMemo<AuthContextValue>(
    () => ({
      isReady,
      isAuthenticated: auth !== null,
      username: auth?.username ?? null,
      role: auth?.role ?? null,
      rights: auth?.rights ?? [],
      hasRight,
      login,
      logout,
    }),
    [isReady, auth, hasRight, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return ctx;
}
