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
}
