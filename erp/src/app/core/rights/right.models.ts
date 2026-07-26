export interface RightSummary {
  id: number;
  code: string;
  module: string;
  description: string;
}

export interface RightGroup {
  module: string;
  rights: RightSummary[];
}
