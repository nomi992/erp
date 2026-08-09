// 1:1 port of erp/src/app/core/branches/branch.models.ts (the admin-facing full Branch shape
// — distinct from tenancy/tenancy.types.ts's BranchSummary, which is just what login returns).
export interface Branch {
  id: number;
  tenantId: number;
  tenantName: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAtUtc: string;
}
