export interface CostCenter {
  id: number;
  name: string;
  parentCostCenterId: number | null;
  parentCostCenterName: string | null;
  isActive: boolean;
}

export interface CostCenterRequest {
  name: string;
  parentCostCenterId: number | null;
}
