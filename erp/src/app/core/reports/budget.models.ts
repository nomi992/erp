export interface Budget {
  id: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  year: number;
  month: number;
  budgetedAmount: number;
}

export interface BudgetRequest {
  accountId: number;
  year: number;
  month: number;
  budgetedAmount: number;
}
