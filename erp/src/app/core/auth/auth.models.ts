import { BranchSummary } from '../tenancy/tenancy.models';

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
