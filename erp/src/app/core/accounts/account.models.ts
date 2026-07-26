export type AccountType = 'Asset' | 'Liability' | 'Equity' | 'Income' | 'Expense';
export type AccountNature = 'Debit' | 'Credit';

export interface Account {
  id: number;
  code: string;
  name: string;
  type: AccountType;
  nature: AccountNature;
  parentAccountId: number | null;
  parentAccountName: string | null;
  isActive: boolean;
  isControlAccount: boolean;
  isCashAccount: boolean;
  openingBalance: number;
  openingBalanceNature: AccountNature;
}

export interface AccountTreeNode {
  id: number;
  code: string;
  name: string;
  type: AccountType;
  nature: AccountNature;
  parentAccountId: number | null;
  isActive: boolean;
  isControlAccount: boolean;
  isCashAccount: boolean;
  openingBalance: number;
  openingBalanceNature: AccountNature;
  children: AccountTreeNode[];
}

export interface CreateAccountRequest {
  code: string;
  name: string;
  type: AccountType;
  nature: AccountNature;
  parentAccountId: number | null;
  isControlAccount: boolean;
  isCashAccount: boolean;
  openingBalance: number;
  openingBalanceNature: AccountNature;
}

export type UpdateAccountRequest = CreateAccountRequest;
