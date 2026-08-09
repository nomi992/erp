import axios from 'axios';
import { API_URL } from '../config';

type TokenAccessor = () => string | null;
type BranchAccessor = () => number | null;
type UnauthorizedHandler = () => void;

let getToken: TokenAccessor = () => null;
let getCurrentBranchId: BranchAccessor = () => null;
let handleUnauthorized: UnauthorizedHandler = () => {};

/**
 * Wired up by AuthProvider once it mounts, so this module (created once, outside the React
 * tree) always reads the latest token. Mirrors erp/src/app/core/auth/auth.interceptor.ts.
 */
export function registerAuthAccessor(fn: TokenAccessor): void {
  getToken = fn;
}

/**
 * Wired up by TenancyProvider once it mounts. Mirrors
 * erp/src/app/core/tenancy/branch.interceptor.ts — we only send X-Branch-Id (no X-Tenant-Id
 * override header) since the mobile app doesn't support the SystemAdmin cross-tenant browsing
 * feature the web app has.
 */
export function registerBranchAccessor(fn: BranchAccessor): void {
  getCurrentBranchId = fn;
}

/** Wired up by AuthProvider — force-logout on 401, same behavior as the web app. */
export function registerUnauthorizedHandler(fn: UnauthorizedHandler): void {
  handleUnauthorized = fn;
}

export const apiClient = axios.create({ baseURL: API_URL });

apiClient.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }

  const branchId = getCurrentBranchId();
  if (branchId != null) {
    config.headers.set('X-Branch-Id', String(branchId));
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      handleUnauthorized();
    }
    return Promise.reject(error);
  },
);
