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

    // --- Stock/Inventory & Invoice Management ---
    public const string ProductCategoriesCreate = "ProductCategories.Create";
    public const string ProductCategoriesEdit = "ProductCategories.Edit";

    public const string UnitsOfMeasureCreate = "UnitsOfMeasure.Create";
    public const string UnitsOfMeasureEdit = "UnitsOfMeasure.Edit";

    public const string ProductsCreate = "Products.Create";
    public const string ProductsEdit = "Products.Edit";

    public const string WarehousesCreate = "Warehouses.Create";
    public const string WarehousesEdit = "Warehouses.Edit";

    public const string StockAccountMappingsManage = "StockAccountMappings.Manage";

    public const string SuppliersCreate = "Suppliers.Create";
    public const string SuppliersEdit = "Suppliers.Edit";
    public const string CustomersCreate = "Customers.Create";
    public const string CustomersEdit = "Customers.Edit";

    public const string PurchaseOrdersCreate = "PurchaseOrders.Create";
    public const string PurchaseOrdersEdit = "PurchaseOrders.Edit";
    public const string PurchaseOrdersSubmit = "PurchaseOrders.Submit";
    public const string PurchaseOrdersApprove = "PurchaseOrders.Approve";
    public const string PurchaseOrdersReject = "PurchaseOrders.Reject";
    public const string PurchaseOrdersCancel = "PurchaseOrders.Cancel";

    public const string SalesOrdersCreate = "SalesOrders.Create";
    public const string SalesOrdersEdit = "SalesOrders.Edit";
    public const string SalesOrdersSubmit = "SalesOrders.Submit";
    public const string SalesOrdersApprove = "SalesOrders.Approve";
    public const string SalesOrdersReject = "SalesOrders.Reject";
    public const string SalesOrdersCancel = "SalesOrders.Cancel";

    public const string PurchaseInvoicesCreate = "PurchaseInvoices.Create";
    public const string PurchaseInvoicesEdit = "PurchaseInvoices.Edit";
    public const string PurchaseInvoicesSubmit = "PurchaseInvoices.Submit";
    public const string PurchaseInvoicesApprove = "PurchaseInvoices.Approve";
    public const string PurchaseInvoicesReject = "PurchaseInvoices.Reject";

    public const string SalesInvoicesCreate = "SalesInvoices.Create";
    public const string SalesInvoicesEdit = "SalesInvoices.Edit";
    public const string SalesInvoicesSubmit = "SalesInvoices.Submit";
    public const string SalesInvoicesApprove = "SalesInvoices.Approve";
    public const string SalesInvoicesReject = "SalesInvoices.Reject";

    public const string PurchaseReturnsCreate = "PurchaseReturns.Create";
    public const string PurchaseReturnsEdit = "PurchaseReturns.Edit";
    public const string PurchaseReturnsSubmit = "PurchaseReturns.Submit";
    public const string PurchaseReturnsApprove = "PurchaseReturns.Approve";
    public const string PurchaseReturnsReject = "PurchaseReturns.Reject";

    public const string SaleReturnsCreate = "SaleReturns.Create";
    public const string SaleReturnsEdit = "SaleReturns.Edit";
    public const string SaleReturnsSubmit = "SaleReturns.Submit";
    public const string SaleReturnsApprove = "SaleReturns.Approve";
    public const string SaleReturnsReject = "SaleReturns.Reject";

    public const string CustomerReceiptsCreate = "CustomerReceipts.Create";
    public const string CustomerReceiptsSubmit = "CustomerReceipts.Submit";
    public const string CustomerReceiptsApprove = "CustomerReceipts.Approve";
    public const string CustomerReceiptsReject = "CustomerReceipts.Reject";

    public const string SupplierPaymentsCreate = "SupplierPayments.Create";
    public const string SupplierPaymentsSubmit = "SupplierPayments.Submit";
    public const string SupplierPaymentsApprove = "SupplierPayments.Approve";
    public const string SupplierPaymentsReject = "SupplierPayments.Reject";

    public const string StockTransfersCreate = "StockTransfers.Create";
    public const string StockTransfersSubmit = "StockTransfers.Submit";
    public const string StockTransfersApprove = "StockTransfers.Approve";
    public const string StockTransfersReject = "StockTransfers.Reject";
    public const string StockTransfersReceive = "StockTransfers.Receive";

    public const string StockAdjustmentsCreate = "StockAdjustments.Create";
    public const string StockAdjustmentsSubmit = "StockAdjustments.Submit";
    public const string StockAdjustmentsApprove = "StockAdjustments.Approve";
    public const string StockAdjustmentsReject = "StockAdjustments.Reject";
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

        (RightCodes.ProductCategoriesCreate, "ProductCategories", "Create new product categories."),
        (RightCodes.ProductCategoriesEdit, "ProductCategories", "Edit product categories, including activating/deactivating."),

        (RightCodes.UnitsOfMeasureCreate, "UnitsOfMeasure", "Create new units of measure and UOM conversions."),
        (RightCodes.UnitsOfMeasureEdit, "UnitsOfMeasure", "Edit units of measure and UOM conversions."),

        (RightCodes.ProductsCreate, "Products", "Create new products and their variants."),
        (RightCodes.ProductsEdit, "Products", "Edit products, variants, and variant pricing."),

        (RightCodes.WarehousesCreate, "Warehouses", "Create new warehouses."),
        (RightCodes.WarehousesEdit, "Warehouses", "Edit warehouses, including activating/deactivating."),

        (RightCodes.StockAccountMappingsManage, "StockAccountMappings", "Manage the GL account mapping used to auto-post stock/invoice transactions."),

        (RightCodes.SuppliersCreate, "Suppliers", "Create new supplier business partners."),
        (RightCodes.SuppliersEdit, "Suppliers", "Edit supplier business partners."),
        (RightCodes.CustomersCreate, "Customers", "Create new customer business partners."),
        (RightCodes.CustomersEdit, "Customers", "Edit customer business partners."),

        (RightCodes.PurchaseOrdersCreate, "PurchaseOrders", "Create new draft purchase orders."),
        (RightCodes.PurchaseOrdersEdit, "PurchaseOrders", "Edit draft purchase orders."),
        (RightCodes.PurchaseOrdersSubmit, "PurchaseOrders", "Submit draft purchase orders for approval."),
        (RightCodes.PurchaseOrdersApprove, "PurchaseOrders", "Approve purchase orders pending approval."),
        (RightCodes.PurchaseOrdersReject, "PurchaseOrders", "Reject purchase orders pending approval."),
        (RightCodes.PurchaseOrdersCancel, "PurchaseOrders", "Cancel an open or partially fulfilled purchase order."),

        (RightCodes.SalesOrdersCreate, "SalesOrders", "Create new draft sales orders."),
        (RightCodes.SalesOrdersEdit, "SalesOrders", "Edit draft sales orders."),
        (RightCodes.SalesOrdersSubmit, "SalesOrders", "Submit draft sales orders for approval."),
        (RightCodes.SalesOrdersApprove, "SalesOrders", "Approve sales orders pending approval."),
        (RightCodes.SalesOrdersReject, "SalesOrders", "Reject sales orders pending approval."),
        (RightCodes.SalesOrdersCancel, "SalesOrders", "Cancel an open or partially fulfilled sales order."),

        (RightCodes.PurchaseInvoicesCreate, "PurchaseInvoices", "Create new draft purchase invoices."),
        (RightCodes.PurchaseInvoicesEdit, "PurchaseInvoices", "Edit draft purchase invoices."),
        (RightCodes.PurchaseInvoicesSubmit, "PurchaseInvoices", "Submit draft purchase invoices for approval."),
        (RightCodes.PurchaseInvoicesApprove, "PurchaseInvoices", "Approve purchase invoices pending approval (posts stock + GL)."),
        (RightCodes.PurchaseInvoicesReject, "PurchaseInvoices", "Reject purchase invoices pending approval."),

        (RightCodes.SalesInvoicesCreate, "SalesInvoices", "Create new draft sales invoices."),
        (RightCodes.SalesInvoicesEdit, "SalesInvoices", "Edit draft sales invoices."),
        (RightCodes.SalesInvoicesSubmit, "SalesInvoices", "Submit draft sales invoices for approval."),
        (RightCodes.SalesInvoicesApprove, "SalesInvoices", "Approve sales invoices pending approval (posts stock + GL)."),
        (RightCodes.SalesInvoicesReject, "SalesInvoices", "Reject sales invoices pending approval."),

        (RightCodes.PurchaseReturnsCreate, "PurchaseReturns", "Create new draft purchase returns."),
        (RightCodes.PurchaseReturnsEdit, "PurchaseReturns", "Edit draft purchase returns."),
        (RightCodes.PurchaseReturnsSubmit, "PurchaseReturns", "Submit draft purchase returns for approval."),
        (RightCodes.PurchaseReturnsApprove, "PurchaseReturns", "Approve purchase returns pending approval (posts stock + GL)."),
        (RightCodes.PurchaseReturnsReject, "PurchaseReturns", "Reject purchase returns pending approval."),

        (RightCodes.SaleReturnsCreate, "SaleReturns", "Create new draft sale returns."),
        (RightCodes.SaleReturnsEdit, "SaleReturns", "Edit draft sale returns."),
        (RightCodes.SaleReturnsSubmit, "SaleReturns", "Submit draft sale returns for approval."),
        (RightCodes.SaleReturnsApprove, "SaleReturns", "Approve sale returns pending approval (posts stock + GL)."),
        (RightCodes.SaleReturnsReject, "SaleReturns", "Reject sale returns pending approval."),

        (RightCodes.CustomerReceiptsCreate, "CustomerReceipts", "Create new draft customer receipts."),
        (RightCodes.CustomerReceiptsSubmit, "CustomerReceipts", "Submit draft customer receipts for approval."),
        (RightCodes.CustomerReceiptsApprove, "CustomerReceipts", "Approve customer receipts pending approval (posts GL, allocates against invoices)."),
        (RightCodes.CustomerReceiptsReject, "CustomerReceipts", "Reject customer receipts pending approval."),

        (RightCodes.SupplierPaymentsCreate, "SupplierPayments", "Create new draft supplier payments."),
        (RightCodes.SupplierPaymentsSubmit, "SupplierPayments", "Submit draft supplier payments for approval."),
        (RightCodes.SupplierPaymentsApprove, "SupplierPayments", "Approve supplier payments pending approval (posts GL, allocates against invoices)."),
        (RightCodes.SupplierPaymentsReject, "SupplierPayments", "Reject supplier payments pending approval."),

        (RightCodes.StockTransfersCreate, "StockTransfers", "Create new draft stock transfers."),
        (RightCodes.StockTransfersSubmit, "StockTransfers", "Submit draft stock transfers for approval."),
        (RightCodes.StockTransfersApprove, "StockTransfers", "Approve stock transfers pending approval."),
        (RightCodes.StockTransfersReject, "StockTransfers", "Reject stock transfers pending approval."),
        (RightCodes.StockTransfersReceive, "StockTransfers", "Confirm receipt of an incoming stock transfer at the destination warehouse."),

        (RightCodes.StockAdjustmentsCreate, "StockAdjustments", "Create new draft stock adjustments, including opening balances."),
        (RightCodes.StockAdjustmentsSubmit, "StockAdjustments", "Submit draft stock adjustments for approval."),
        (RightCodes.StockAdjustmentsApprove, "StockAdjustments", "Approve stock adjustments pending approval (posts stock + GL)."),
        (RightCodes.StockAdjustmentsReject, "StockAdjustments", "Reject stock adjustments pending approval."),
    ];
}
