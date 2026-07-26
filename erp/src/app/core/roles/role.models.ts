import { RightSummary } from '../rights/right.models';

export interface RoleSummary {
  id: number;
  name: string;
  isSystemRole: boolean;
}

export interface RoleResponse {
  id: number;
  tenantId: number | null;
  tenantName: string | null;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  createdAtUtc: string;
  rights: RightSummary[];
}

export interface RoleRequest {
  tenantId?: number | null;
  name: string;
  description?: string | null;
  rightIds: number[];
}
