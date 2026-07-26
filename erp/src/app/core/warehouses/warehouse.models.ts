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

export interface WarehouseRequest {
  code: string;
  name: string;
  costCenterId: number | null;
  isDefault: boolean;
}
