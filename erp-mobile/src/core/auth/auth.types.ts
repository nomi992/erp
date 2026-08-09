import type { BranchSummary } from '../tenancy/tenancy.types';

// 1:1 port of erp/src/app/core/auth/auth.models.ts.
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  expiresAtUtc: string;
  tenantId: number;
  tenantName: string;
  branches: BranchSummary[];
  rights: string[];
}

// What we persist to AsyncStorage — mirrors the StoredAuth shape kept in
// erp/src/app/core/auth/auth.service.ts's localStorage blob (key "erp.auth").
export interface StoredAuth {
  token: string;
  username: string;
  role: string;
  expiresAtUtc: string;
  rights: string[];
}
