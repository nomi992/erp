// Subset of erp/src/app/core/accounts/account.models.ts needed for the voucher line
// account picker — the mobile app doesn't manage the Chart of Accounts itself.
export type AccountType = 'Asset' | 'Liability' | 'Equity' | 'Income' | 'Expense';
export type AccountNature = 'Debit' | 'Credit';

export interface Account {
  id: number;
  code: string;
  name: string;
  type: AccountType;
  nature: AccountNature;
  isActive: boolean;
}
