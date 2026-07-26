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

import { ProductCategories } from './pages/product-categories/product-categories';
import { UnitsOfMeasure } from './pages/units-of-measure/units-of-measure';
import { Warehouses } from './pages/warehouses/warehouses';
import { StockAccountMappings } from './pages/stock-account-mappings/stock-account-mappings';
import { ProductList } from './pages/products/product-list/product-list';
import { ProductForm } from './pages/products/product-form/product-form';
import { Suppliers } from './pages/suppliers/suppliers';
import { Customers } from './pages/customers/customers';

import { PurchaseOrderList } from './pages/purchase-orders/purchase-order-list';
import { PurchaseOrderForm } from './pages/purchase-orders/purchase-order-form';
import { PurchaseInvoiceList } from './pages/purchase-invoices/purchase-invoice-list';
import { PurchaseInvoiceForm } from './pages/purchase-invoices/purchase-invoice-form';
import { PurchaseReturnList } from './pages/purchase-returns/purchase-return-list';
import { PurchaseReturnForm } from './pages/purchase-returns/purchase-return-form';
import { SalesOrderList } from './pages/sales-orders/sales-order-list';
import { SalesOrderForm } from './pages/sales-orders/sales-order-form';
import { SalesInvoiceList } from './pages/sales-invoices/sales-invoice-list';
import { SalesInvoiceForm } from './pages/sales-invoices/sales-invoice-form';
import { SaleReturnList } from './pages/sale-returns/sale-return-list';
import { SaleReturnForm } from './pages/sale-returns/sale-return-form';

import { CustomerReceiptList } from './pages/customer-receipts/customer-receipt-list';
import { CustomerReceiptForm } from './pages/customer-receipts/customer-receipt-form';
import { SupplierPaymentList } from './pages/supplier-payments/supplier-payment-list';
import { SupplierPaymentForm } from './pages/supplier-payments/supplier-payment-form';

import { StockTransferList } from './pages/stock-transfers/stock-transfer-list/stock-transfer-list';
import { StockTransferForm } from './pages/stock-transfers/stock-transfer-form/stock-transfer-form';
import { StockAdjustmentList } from './pages/stock-adjustments/stock-adjustment-list/stock-adjustment-list';
import { StockAdjustmentForm } from './pages/stock-adjustments/stock-adjustment-form/stock-adjustment-form';

import { StockLedgerPage } from './pages/stock-ledger/stock-ledger';
import { StockOnHandPage } from './pages/stock-on-hand/stock-on-hand';
import { AccountsReceivableAging } from './pages/accounts-receivable-aging/accounts-receivable-aging';
import { AccountsPayableAging } from './pages/accounts-payable-aging/accounts-payable-aging';
import { ThemeSettings } from './pages/settings/theme-settings/theme-settings';

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

      // --- Stock/Inventory & Invoice Management ---
      { path: 'product-categories', component: ProductCategories },
      { path: 'units-of-measure', component: UnitsOfMeasure },
      { path: 'warehouses', component: Warehouses },
      { path: 'stock-account-mappings', component: StockAccountMappings },
      { path: 'products', component: ProductList },
      { path: 'products/new', component: ProductForm },
      { path: 'products/:id/edit', component: ProductForm, data: { mode: 'edit' } },
      { path: 'suppliers', component: Suppliers },
      { path: 'customers', component: Customers },

      { path: 'purchase-orders', component: PurchaseOrderList },
      { path: 'purchase-orders/new', component: PurchaseOrderForm },
      { path: 'purchase-orders/:id/edit', component: PurchaseOrderForm, data: { mode: 'edit' } },
      { path: 'purchase-orders/:id', component: PurchaseOrderForm, data: { mode: 'view' } },

      { path: 'purchase-invoices', component: PurchaseInvoiceList },
      { path: 'purchase-invoices/new', component: PurchaseInvoiceForm },
      { path: 'purchase-invoices/:id/edit', component: PurchaseInvoiceForm, data: { mode: 'edit' } },
      { path: 'purchase-invoices/:id', component: PurchaseInvoiceForm, data: { mode: 'view' } },

      { path: 'purchase-returns', component: PurchaseReturnList },
      { path: 'purchase-returns/new', component: PurchaseReturnForm },
      { path: 'purchase-returns/:id/edit', component: PurchaseReturnForm, data: { mode: 'edit' } },
      { path: 'purchase-returns/:id', component: PurchaseReturnForm, data: { mode: 'view' } },

      { path: 'sales-orders', component: SalesOrderList },
      { path: 'sales-orders/new', component: SalesOrderForm },
      { path: 'sales-orders/:id/edit', component: SalesOrderForm, data: { mode: 'edit' } },
      { path: 'sales-orders/:id', component: SalesOrderForm, data: { mode: 'view' } },

      { path: 'sales-invoices', component: SalesInvoiceList },
      { path: 'sales-invoices/new', component: SalesInvoiceForm },
      { path: 'sales-invoices/:id/edit', component: SalesInvoiceForm, data: { mode: 'edit' } },
      { path: 'sales-invoices/:id', component: SalesInvoiceForm, data: { mode: 'view' } },

      { path: 'sale-returns', component: SaleReturnList },
      { path: 'sale-returns/new', component: SaleReturnForm },
      { path: 'sale-returns/:id/edit', component: SaleReturnForm, data: { mode: 'edit' } },
      { path: 'sale-returns/:id', component: SaleReturnForm, data: { mode: 'view' } },

      { path: 'customer-receipts', component: CustomerReceiptList },
      { path: 'customer-receipts/new', component: CustomerReceiptForm },
      { path: 'customer-receipts/:id', component: CustomerReceiptForm },

      { path: 'supplier-payments', component: SupplierPaymentList },
      { path: 'supplier-payments/new', component: SupplierPaymentForm },
      { path: 'supplier-payments/:id', component: SupplierPaymentForm },

      { path: 'stock-transfers', component: StockTransferList },
      { path: 'stock-transfers/new', component: StockTransferForm },
      { path: 'stock-transfers/:id', component: StockTransferForm },

      { path: 'stock-adjustments', component: StockAdjustmentList },
      { path: 'stock-adjustments/new', component: StockAdjustmentForm },
      { path: 'stock-adjustments/:id', component: StockAdjustmentForm },

      { path: 'stock-ledger', component: StockLedgerPage },
      { path: 'stock-on-hand', component: StockOnHandPage },
      { path: 'accounts-receivable-aging', component: AccountsReceivableAging },
      { path: 'accounts-payable-aging', component: AccountsPayableAging },

      { path: 'settings/theme', component: ThemeSettings },

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
