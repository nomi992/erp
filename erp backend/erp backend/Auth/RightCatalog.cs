namespace erp_backend.Auth;

public static class RightCodes
{
    public const string AccountsCreate = "Accounts.Create";
    public const string AccountsEdit = "Accounts.Edit";
    public const string AccountsDelete = "Accounts.Delete";

    public const string CostCentersCreate = "CostCenters.Create";
    public const string CostCentersEdit = "CostCenters.Edit";
    public const string CostCentersDelete = "CostCenters.Delete";

    public const string TaxRatesCreate = "TaxRates.Create";
    public const string TaxRatesEdit = "TaxRates.Edit";

    public const string FiscalPeriodsCreate = "FiscalPeriods.Create";
    public const string FiscalPeriodsClose = "FiscalPeriods.Close";
    public const string FiscalPeriodsReopen = "FiscalPeriods.Reopen";

    public const string VouchersCreate = "Vouchers.Create";
    public const string VouchersEdit = "Vouchers.Edit";
    public const string VouchersSubmit = "Vouchers.Submit";
    public const string VouchersManageAttachments = "Vouchers.ManageAttachments";
    public const string VouchersApprove = "Vouchers.Approve";
    public const string VouchersReject = "Vouchers.Reject";
    public const string VouchersReverse = "Vouchers.Reverse";

    public const string RecurringVoucherTemplatesCreate = "RecurringVoucherTemplates.Create";
    public const string RecurringVoucherTemplatesEdit = "RecurringVoucherTemplates.Edit";
    public const string RecurringVoucherTemplatesGenerateNow = "RecurringVoucherTemplates.GenerateNow";

    public const string BudgetsCreate = "Budgets.Create";
    public const string BudgetsEdit = "Budgets.Edit";
    public const string BudgetsDelete = "Budgets.Delete";

    public const string ReportSchedulesCreate = "ReportSchedules.Create";
    public const string ReportSchedulesEdit = "ReportSchedules.Edit";
    public const string ReportSchedulesDelete = "ReportSchedules.Delete";
    public const string ReportSchedulesRunNow = "ReportSchedules.RunNow";

    public const string BranchesCreate = "Branches.Create";
    public const string BranchesEdit = "Branches.Edit";

    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersManageBranchAccess = "Users.ManageBranchAccess";

    public const string RolesManage = "Roles.Manage";
}

public static class RightCatalog
{
    public static readonly IReadOnlyList<(string Code, string Module, string Description)> All =
    [
        (RightCodes.AccountsCreate, "Accounts", "Create new chart of accounts entries."),
        (RightCodes.AccountsEdit, "Accounts", "Edit chart of accounts entries, including activating/deactivating."),
        (RightCodes.AccountsDelete, "Accounts", "Delete chart of accounts entries."),

        (RightCodes.CostCentersCreate, "CostCenters", "Create new cost centers."),
        (RightCodes.CostCentersEdit, "CostCenters", "Edit cost centers, including activating/deactivating."),
        (RightCodes.CostCentersDelete, "CostCenters", "Delete cost centers."),

        (RightCodes.TaxRatesCreate, "TaxRates", "Create new tax rates."),
        (RightCodes.TaxRatesEdit, "TaxRates", "Edit tax rates, including activating/deactivating."),

        (RightCodes.FiscalPeriodsCreate, "FiscalPeriods", "Create new fiscal periods."),
        (RightCodes.FiscalPeriodsClose, "FiscalPeriods", "Close a fiscal period."),
        (RightCodes.FiscalPeriodsReopen, "FiscalPeriods", "Reopen a closed fiscal period."),

        (RightCodes.VouchersCreate, "Vouchers", "Create new draft vouchers."),
        (RightCodes.VouchersEdit, "Vouchers", "Edit draft vouchers."),
        (RightCodes.VouchersSubmit, "Vouchers", "Submit draft vouchers for approval."),
        (RightCodes.VouchersManageAttachments, "Vouchers", "Upload and delete voucher attachments."),
        (RightCodes.VouchersApprove, "Vouchers", "Approve vouchers pending approval."),
        (RightCodes.VouchersReject, "Vouchers", "Reject vouchers pending approval."),
        (RightCodes.VouchersReverse, "Vouchers", "Reverse posted vouchers."),

        (RightCodes.RecurringVoucherTemplatesCreate, "RecurringVoucherTemplates", "Create new recurring voucher templates."),
        (RightCodes.RecurringVoucherTemplatesEdit, "RecurringVoucherTemplates", "Edit recurring voucher templates, including activating/deactivating."),
        (RightCodes.RecurringVoucherTemplatesGenerateNow, "RecurringVoucherTemplates", "Manually trigger generation of a voucher from a template."),

        (RightCodes.BudgetsCreate, "Budgets", "Create new budgets."),
        (RightCodes.BudgetsEdit, "Budgets", "Edit budgets."),
        (RightCodes.BudgetsDelete, "Budgets", "Delete budgets."),

        (RightCodes.ReportSchedulesCreate, "ReportSchedules", "Create new report schedules."),
        (RightCodes.ReportSchedulesEdit, "ReportSchedules", "Edit report schedules, including activating/deactivating."),
        (RightCodes.ReportSchedulesDelete, "ReportSchedules", "Delete report schedules."),
        (RightCodes.ReportSchedulesRunNow, "ReportSchedules", "Manually run a report schedule immediately."),

        (RightCodes.BranchesCreate, "Branches", "Create new branches."),
        (RightCodes.BranchesEdit, "Branches", "Edit branches, including activating/deactivating."),

        (RightCodes.UsersView, "Users", "View the list of users and their details."),
        (RightCodes.UsersCreate, "Users", "Create new users."),
        (RightCodes.UsersEdit, "Users", "Edit users, including activating/deactivating and changing their role."),
        (RightCodes.UsersManageBranchAccess, "Users", "Grant or revoke a user's access to branches."),

        (RightCodes.RolesManage, "Roles", "Create, update, and delete roles, and assign rights to them."),
    ];
}
