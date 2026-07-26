export interface Branch {
  id: number;
  tenantId: number;
  tenantName: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface BranchRequest {
  tenantId?: number | null;
  name: string;
  code: string;
}
