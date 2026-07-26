import { AccountNature } from '../accounts/account.models';
import { VoucherType } from '../vouchers/voucher.models';

export interface GeneralLedgerEntry {
  voucherId: number;
  voucherNo: string;
  voucherType: VoucherType;
  date: string;
  narration: string;
  accountId: number;
  accountCode: string;
  accountName: string;
  debitAmount: number;
  creditAmount: number;
}

export interface AccountLedgerEntry {
  voucherId: number;
  voucherNo: string;
  date: string;
  narration: string;
  debitAmount: number;
  creditAmount: number;
  runningBalance: number;
  runningBalanceNature: AccountNature;
}

export interface AccountLedger {
  accountId: number;
  accountCode: string;
  accountName: string;
  openingBalance: number;
  openingBalanceNature: AccountNature;
  closingBalance: number;
  closingBalanceNature: AccountNature;
  entries: AccountLedgerEntry[];
}

export type SubLedgerType = 'Receivable' | 'Payable';

export interface SubLedgerEntry {
  accountId: number;
  accountCode: string;
  accountName: string;
  outstandingBalance: number;
  outstandingBalanceNature: AccountNature;
  lastActivityDate: string | null;
  ageInDays: number;
  ageBucket: string;
}

export interface BankStatementLine {
  id: number;
  transactionDate: string;
  amount: number;
  description: string;
  referenceNo: string;
  isMatched: boolean;
  matchedVoucherDetailId: number | null;
  matchedVoucherId: number | null;
  matchedVoucherNo: string | null;
}
