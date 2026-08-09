// 1:1 port of erp/src/app/core/tax-rates/tax-rate.models.ts.
export interface TaxRate {
  id: number;
  name: string;
  percentage: number;
  isActive: boolean;
}
