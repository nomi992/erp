export interface UnitOfMeasure {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
}

export interface UnitOfMeasureRequest {
  code: string;
  name: string;
}
