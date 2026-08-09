// Subset of erp/src/app/core/roles/role.models.ts needed for the Role picker on the Users
// admin screen — the mobile app doesn't edit a Role's own rights (see README.md scoping).
export interface RoleOption {
  id: number;
  name: string;
  isSystemRole: boolean;
}
