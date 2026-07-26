import { VoucherType } from '../vouchers/voucher.models';

export interface TrialBalanceRow {
  accountId: number;
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
}

export interface TrialBalance {
  asOf: string;
  rows: TrialBalanceRow[];
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
}

export interface ProfitLossRow {
  accountId: number;
  accountCode: string;
  accountName: string;
  amount: number;
  priorAmount: number | null;
}

export interface ProfitLoss {
  from: string;
  to: string;
  income: ProfitLossRow[];
  expenses: ProfitLossRow[];
  totalIncome: number;
  totalExpenses: number;
  netProfit: number;
  priorTotalIncome: number | null;
  priorTotalExpenses: number | null;
  priorNetProfit: number | null;
}

export interface BalanceSheetRow {
  accountId: number;
  accountCode: string;
  accountName: string;
  amount: number;
}

export interface BalanceSheet {
  asOf: string;
  assets: BalanceSheetRow[];
  liabilities: BalanceSheetRow[];
  equity: BalanceSheetRow[];
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  isBalanced: boolean;
}

export interface CashFlowLine {
  category: 'Operating' | 'Investing' | 'Financing';
  accountId: number;
  accountCode: string;
  accountName: string;
  amount: number;
}

export interface CashFlow {
  from: string;
  to: string;
  operatingActivities: number;
  investingActivities: number;
  financingActivities: number;
  netCashFlow: number;
  lines: CashFlowLine[];
}

export interface DayBookLine {
  accountCode: string;
  accountName: string;
  debitAmount: number;
  creditAmount: number;
}

export interface DayBookEntry {
  voucherId: number;
  voucherNo: string;
  voucherType: VoucherType;
  date: string;
  narration: string;
  lines: DayBookLine[];
}

export interface BudgetVsActualRow {
  accountId: number;
  accountCode: string;
  accountName: string;
  budgeted: number;
  actual: number;
  variance: number;
}
