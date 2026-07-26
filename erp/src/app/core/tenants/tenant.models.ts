export interface Tenant {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
  createdAtUtc: string;
  branchCount: number;
  userCount: number;
}

export interface CreateTenantRequest {
  name: string;
  code: string;
}

export interface UpdateTenantRequest {
  name: string;
}
