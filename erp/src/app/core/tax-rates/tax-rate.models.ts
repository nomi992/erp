export interface TaxRate {
  id: number;
  name: string;
  percentage: number;
  isActive: boolean;
}

export interface TaxRateRequest {
  name: string;
  percentage: number;
}
