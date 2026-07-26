import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Shell } from './layout/shell/shell';
import { Dashboard } from './pages/dashboard/dashboard';
import { ChartOfAccounts } from './pages/chart-of-accounts/chart-of-accounts';
import { CostCenters } from './pages/cost-centers/cost-centers';
import { FiscalPeriods } from './pages/fiscal-periods/fiscal-periods';
import { TaxRates } from './pages/tax-rates/tax-rates';
import { VoucherList } from './pages/vouchers/voucher-list/voucher-list';
import { VoucherForm } from './pages/vouchers/voucher-form/voucher-form';
import { RecurringVouchers } from './pages/recurring-vouchers/recurring-vouchers';
import { GeneralLedger } from './pages/ledgers/general-ledger/general-ledger';
import { AccountLedgerPage } from './pages/ledgers/account-ledger/account-ledger';
import { SubLedger } from './pages/ledgers/sub-ledger/sub-ledger';
import { BankReconciliation } from './pages/bank-reconciliation/bank-reconciliation';
import { TrialBalanceReport } from './pages/reports/trial-balance/trial-balance';
import { ProfitLossReport } from './pages/reports/profit-loss/profit-loss';
import { BalanceSheetReport } from './pages/reports/balance-sheet/balance-sheet';
import { CashFlowReport } from './pages/reports/cash-flow/cash-flow';
import { AgingReport } from './pages/reports/aging/aging';
import { DayBookReport } from './pages/reports/day-book/day-book';
import { BudgetVsActualReport } from './pages/reports/budget-vs-actual/budget-vs-actual';
import { Budgets } from './pages/budgets/budgets';
import { ReportSchedules } from './pages/report-schedules/report-schedules';
import { Tenants } from './pages/admin/tenants/tenants';
import { Branches } from './pages/admin/branches/branches';
import { Users } from './pages/admin/users/users';
import { Roles } from './pages/admin/roles/roles';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'chart-of-accounts', component: ChartOfAccounts },
      { path: 'cost-centers', component: CostCenters },
      { path: 'fiscal-periods', component: FiscalPeriods },
      { path: 'tax-rates', component: TaxRates },

      { path: 'vouchers', component: VoucherList },
      { path: 'vouchers/new', component: VoucherForm },
      { path: 'vouchers/:id/edit', component: VoucherForm, data: { mode: 'edit' } },
      { path: 'vouchers/:id', component: VoucherForm, data: { mode: 'view' } },
      { path: 'recurring-vouchers', component: RecurringVouchers },

      { path: 'ledgers/general', component: GeneralLedger },
      { path: 'ledgers/account', component: AccountLedgerPage },
      { path: 'ledgers/sub-ledger', component: SubLedger },
      { path: 'bank-reconciliation', component: BankReconciliation },

      { path: 'reports/trial-balance', component: TrialBalanceReport },
      { path: 'reports/profit-loss', component: ProfitLossReport },
      { path: 'reports/balance-sheet', component: BalanceSheetReport },
      { path: 'reports/cash-flow', component: CashFlowReport },
      { path: 'reports/aging', component: AgingReport },
      { path: 'reports/day-book', component: DayBookReport },
      { path: 'reports/budget-vs-actual', component: BudgetVsActualReport },
      { path: 'budgets', component: Budgets },
      { path: 'report-schedules', component: ReportSchedules },

      {
        path: 'admin/tenants',
        component: Tenants,
        canActivate: [roleGuard],
        data: { roles: ['SystemAdmin'] },
      },
      {
        path: 'admin/branches',
        component: Branches,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'SystemAdmin'] },
      },
      {
        path: 'admin/users',
        component: Users,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'SystemAdmin'], rights: ['Users.View'] },
      },
      {
        path: 'admin/roles',
        component: Roles,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'SystemAdmin'] },
      },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
];
