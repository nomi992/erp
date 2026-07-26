export interface FiscalPeriod {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  isClosed: boolean;
}

export interface FiscalPeriodRequest {
  name: string;
  startDate: string;
  endDate: string;
}
