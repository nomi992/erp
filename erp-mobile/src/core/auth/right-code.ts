// 1:1 port of erp/src/app/core/auth/right-code.ts, which itself mirrors the backend's
// Auth/RightCodes.cs. Keep these two frontends in sync by hand whenever the backend adds a
// right — there is no shared package between them.
export enum RightCode {
  AccountsCreate = 'Accounts.Create',
  AccountsEdit = 'Accounts.Edit',
  AccountsDelete = 'Accounts.Delete',

  CostCentersCreate = 'CostCenters.Create',
  CostCentersEdit = 'CostCenters.Edit',
  CostCentersDelete = 'CostCenters.Delete',

  TaxRatesCreate = 'TaxRates.Create',
  TaxRatesEdit = 'TaxRates.Edit',

  FiscalPeriodsCreate = 'FiscalPeriods.Create',
  FiscalPeriodsClose = 'FiscalPeriods.Close',
  FiscalPeriodsReopen = 'FiscalPeriods.Reopen',

  VouchersCreate = 'Vouchers.Create',
  VouchersEdit = 'Vouchers.Edit',
  VouchersSubmit = 'Vouchers.Submit',
  VouchersManageAttachments = 'Vouchers.ManageAttachments',
  VouchersApprove = 'Vouchers.Approve',
  VouchersReject = 'Vouchers.Reject',
  VouchersReverse = 'Vouchers.Reverse',

  RecurringVoucherTemplatesCreate = 'RecurringVoucherTemplates.Create',
  RecurringVoucherTemplatesEdit = 'RecurringVoucherTemplates.Edit',
  RecurringVoucherTemplatesGenerateNow = 'RecurringVoucherTemplates.GenerateNow',

  BudgetsCreate = 'Budgets.Create',
  BudgetsEdit = 'Budgets.Edit',
  BudgetsDelete = 'Budgets.Delete',

  ReportSchedulesCreate = 'ReportSchedules.Create',
  ReportSchedulesEdit = 'ReportSchedules.Edit',
  ReportSchedulesDelete = 'ReportSchedules.Delete',
  ReportSchedulesRunNow = 'ReportSchedules.RunNow',

  BranchesCreate = 'Branches.Create',
  BranchesEdit = 'Branches.Edit',

  UsersView = 'Users.View',
  UsersCreate = 'Users.Create',
  UsersEdit = 'Users.Edit',
  UsersManageBranchAccess = 'Users.ManageBranchAccess',

  RolesManage = 'Roles.Manage',

  // --- Stock/Inventory & Invoice Management ---
  ProductCategoriesCreate = 'ProductCategories.Create',
  ProductCategoriesEdit = 'ProductCategories.Edit',

  UnitsOfMeasureCreate = 'UnitsOfMeasure.Create',
  UnitsOfMeasureEdit = 'UnitsOfMeasure.Edit',

  ProductsCreate = 'Products.Create',
  ProductsEdit = 'Products.Edit',

  WarehousesCreate = 'Warehouses.Create',
  WarehousesEdit = 'Warehouses.Edit',

  StockAccountMappingsManage = 'StockAccountMappings.Manage',

  SuppliersCreate = 'Suppliers.Create',
  SuppliersEdit = 'Suppliers.Edit',
  CustomersCreate = 'Customers.Create',
  CustomersEdit = 'Customers.Edit',

  PurchaseOrdersCreate = 'PurchaseOrders.Create',
  PurchaseOrdersEdit = 'PurchaseOrders.Edit',
  PurchaseOrdersSubmit = 'PurchaseOrders.Submit',
  PurchaseOrdersApprove = 'PurchaseOrders.Approve',
  PurchaseOrdersReject = 'PurchaseOrders.Reject',
  PurchaseOrdersCancel = 'PurchaseOrders.Cancel',

  SalesOrdersCreate = 'SalesOrders.Create',
  SalesOrdersEdit = 'SalesOrders.Edit',
  SalesOrdersSubmit = 'SalesOrders.Submit',
  SalesOrdersApprove = 'SalesOrders.Approve',
  SalesOrdersReject = 'SalesOrders.Reject',
  SalesOrdersCancel = 'SalesOrders.Cancel',

  PurchaseInvoicesCreate = 'PurchaseInvoices.Create',
  PurchaseInvoicesEdit = 'PurchaseInvoices.Edit',
  PurchaseInvoicesSubmit = 'PurchaseInvoices.Submit',
  PurchaseInvoicesApprove = 'PurchaseInvoices.Approve',
  PurchaseInvoicesReject = 'PurchaseInvoices.Reject',

  SalesInvoicesCreate = 'SalesInvoices.Create',
  SalesInvoicesEdit = 'SalesInvoices.Edit',
  SalesInvoicesSubmit = 'SalesInvoices.Submit',
  SalesInvoicesApprove = 'SalesInvoices.Approve',
  SalesInvoicesReject = 'SalesInvoices.Reject',

  PurchaseReturnsCreate = 'PurchaseReturns.Create',
  PurchaseReturnsEdit = 'PurchaseReturns.Edit',
  PurchaseReturnsSubmit = 'PurchaseReturns.Submit',
  PurchaseReturnsApprove = 'PurchaseReturns.Approve',
  PurchaseReturnsReject = 'PurchaseReturns.Reject',

  SaleReturnsCreate = 'SaleReturns.Create',
  SaleReturnsEdit = 'SaleReturns.Edit',
  SaleReturnsSubmit = 'SaleReturns.Submit',
  SaleReturnsApprove = 'SaleReturns.Approve',
  SaleReturnsReject = 'SaleReturns.Reject',

  CustomerReceiptsCreate = 'CustomerReceipts.Create',
  CustomerReceiptsSubmit = 'CustomerReceipts.Submit',
  CustomerReceiptsApprove = 'CustomerReceipts.Approve',
  CustomerReceiptsReject = 'CustomerReceipts.Reject',

  SupplierPaymentsCreate = 'SupplierPayments.Create',
  SupplierPaymentsSubmit = 'SupplierPayments.Submit',
  SupplierPaymentsApprove = 'SupplierPayments.Approve',
  SupplierPaymentsReject = 'SupplierPayments.Reject',

  StockTransfersCreate = 'StockTransfers.Create',
  StockTransfersSubmit = 'StockTransfers.Submit',
  StockTransfersApprove = 'StockTransfers.Approve',
  StockTransfersReject = 'StockTransfers.Reject',
  StockTransfersReceive = 'StockTransfers.Receive',

  StockAdjustmentsCreate = 'StockAdjustments.Create',
  StockAdjustmentsSubmit = 'StockAdjustments.Submit',
  StockAdjustmentsApprove = 'StockAdjustments.Approve',
  StockAdjustmentsReject = 'StockAdjustments.Reject',
}
