// 1:1 port of erp/src/app/core/warehouses/warehouse.models.ts.
export interface Warehouse {
  id: number;
  code: string;
  name: string;
  costCenterId: number | null;
  costCenterName: string | null;
  isDefault: boolean;
  isActive: boolean;
  createdAtUtc: string;
}
