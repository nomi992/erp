import { BranchSummary } from '../tenancy/tenancy.models';

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
  tenantId?: number | null;
  username: string;
  password: string;
  roleId: number;
  branchIds: number[];
}

export interface UpdateUserRequest {
  roleId: number;
  isActive: boolean;
}
