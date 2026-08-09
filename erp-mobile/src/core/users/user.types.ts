import type { BranchSummary } from '../tenancy/tenancy.types';

// 1:1 port of erp/src/app/core/users/user.models.ts.
export interface AdminUser {
  id: number;
  tenantId: number;
  tenantName: string;
  username: string;
  roleId: number;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
  branches: BranchSummary[];
}

export interface CreateUserRequest {
  username: string;
  password: string;
  roleId: number;
  branchIds: number[];
}

export interface UpdateUserRequest {
  roleId: number;
  isActive: boolean;
}
