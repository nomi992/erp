using System.ComponentModel;
using System.Reflection;

namespace erp_backend.Messages;

/// <summary>
/// Every user-facing message the API sends to the frontend, in one place. Members whose text contains
/// "{0}", "{1}", etc. are formatted via <see cref="ResponseMessageExtensions.ToText"/>'s params args.
/// </summary>
public enum ResponseMessage
{
    [Description("Request successful")]
    Success,

    [Description("An unexpected error occurred.")]
    UnknownError,

    // --- Accounts ---
    [Description("Account not found.")]
    AccountNotFound,
    [Description("An account with this code already exists.")]
    AccountCodeExists,
    [Description("Parent account not found.")]
    ParentAccountNotFound,
    [Description("An account cannot be its own parent.")]
    AccountCannotBeOwnParent,
    [Description("This parent assignment would create a circular reference.")]
    CircularReference,
    [Description("Account created successfully.")]
    AccountCreated,
    [Description("Account updated successfully.")]
    AccountUpdated,
    [Description("This account has child accounts and cannot be deleted. Deactivate it instead.")]
    AccountHasChildren,
    [Description("This account has associated transactions and cannot be deleted. Deactivate it instead.")]
    AccountHasTransactions,
    [Description("Account deleted successfully.")]
    AccountDeleted,
    [Description("Account activated.")]
    AccountActivated,
    [Description("Account deactivated.")]
    AccountDeactivated,

    // --- Cost Centers ---
    [Description("Cost center not found.")]
    CostCenterNotFound,
    [Description("Parent cost center not found.")]
    ParentCostCenterNotFound,
    [Description("A cost center cannot be its own parent.")]
    CostCenterCannotBeOwnParent,
    [Description("Cost center created successfully.")]
    CostCenterCreated,
    [Description("Cost center updated successfully.")]
    CostCenterUpdated,
    [Description("This cost center has child cost centers and cannot be deleted. Deactivate it instead.")]
    CostCenterHasChildren,
    [Description("This cost center is used by voucher entries and cannot be deleted. Deactivate it instead.")]
    CostCenterHasTransactions,
    [Description("Cost center deleted successfully.")]
    CostCenterDeleted,
    [Description("Cost center activated.")]
    CostCenterActivated,
    [Description("Cost center deactivated.")]
    CostCenterDeactivated,

    // --- Fiscal Periods ---
    [Description("Fiscal period not found.")]
    FiscalPeriodNotFound,
    [Description("End date must be on or after the start date.")]
    FiscalPeriodEndBeforeStart,
    [Description("This period overlaps with an existing fiscal period.")]
    FiscalPeriodOverlaps,
    [Description("Fiscal period created successfully.")]
    FiscalPeriodCreated,
    [Description("Fiscal period closed.")]
    FiscalPeriodClosed,
    [Description("Fiscal period reopened.")]
    FiscalPeriodReopened,

    // --- Tax Rates ---
    [Description("Tax rate not found.")]
    TaxRateNotFound,
    [Description("Tax rate created successfully.")]
    TaxRateCreated,
    [Description("Tax rate updated successfully.")]
    TaxRateUpdated,
    [Description("Tax rate activated.")]
    TaxRateActivated,
    [Description("Tax rate deactivated.")]
    TaxRateDeactivated,

    // --- Vouchers ---
    [Description("Voucher not found.")]
    VoucherNotFound,
    [Description("A voucher requires at least two line items.")]
    VoucherRequiresTwoLines,
    [Description("Amounts cannot be negative.")]
    VoucherAmountsNegative,
    [Description("Each line must have a debit or credit amount.")]
    VoucherLineRequiresAmount,
    [Description("A line cannot have both a debit and a credit amount.")]
    VoucherLineBothAmounts,
    [Description("Account {0} was not found.")]
    VoucherLineAccountNotFound,
    [Description("Cost center {0} was not found.")]
    VoucherLineCostCenterNotFound,
    [Description("Tax rates only apply to Sales and Purchase vouchers.")]
    VoucherTaxRateWrongType,
    [Description("Tax rate {0} was not found.")]
    VoucherLineTaxRateNotFound,
    [Description("Total debit ({0}) must equal total credit ({1}).")]
    VoucherUnbalanced,
    [Description("This date falls within a closed fiscal period.")]
    VoucherDateInClosedPeriod,
    [Description("Only draft vouchers can be edited.")]
    VoucherNotDraftForEdit,
    [Description("Voucher created as draft.")]
    VoucherCreated,
    [Description("Voucher updated.")]
    VoucherUpdated,
    [Description("Only draft vouchers can be submitted for approval.")]
    VoucherNotDraftForSubmit,
    [Description("Voucher submitted for approval.")]
    VoucherSubmitted,
    [Description("Only vouchers pending approval can be approved.")]
    VoucherNotPendingForApprove,
    [Description("This voucher's date falls within a closed fiscal period.")]
    VoucherDateInClosedPeriodForApproval,
    [Description("Voucher approved and posted.")]
    VoucherApproved,
    [Description("Only vouchers pending approval can be rejected.")]
    VoucherNotPendingForReject,
    [Description("Voucher rejected.")]
    VoucherRejected,
    [Description("Only posted vouchers can be reversed.")]
    VoucherNotPostedForReverse,
    [Description("Today's date falls within a closed fiscal period.")]
    TodayInClosedPeriod,
    [Description("Voucher reversed successfully.")]
    VoucherReversed,
    [Description("The uploaded file is empty.")]
    AttachmentFileEmpty,
    [Description("Attachment uploaded.")]
    AttachmentUploaded,
    [Description("Attachment not found.")]
    AttachmentNotFound,
    [Description("Attachment file is missing on disk.")]
    AttachmentFileMissing,
    [Description("Attachment deleted.")]
    AttachmentDeleted,

    // --- Recurring Voucher Templates ---
    [Description("Template not found.")]
    TemplateNotFound,
    [Description("A template requires at least two line items.")]
    TemplateRequiresTwoLines,
    [Description("Recurring template created successfully.")]
    TemplateCreated,
    [Description("Recurring template updated successfully.")]
    TemplateUpdated,
    [Description("This template is inactive.")]
    TemplateInactive,
    [Description("Template activated.")]
    TemplateActivated,
    [Description("Template deactivated.")]
    TemplateDeactivated,
    [Description("Draft voucher {0} generated. Next run date advanced to {1}.")]
    TemplateGenerated,

    // --- Ledgers / Bank Reconciliation ---
    [Description("Bank account not found.")]
    BankAccountNotFound,
    [Description("Imported {0} bank statement lines.")]
    BankStatementImported,
    [Description("Auto-matched {0} of {1} unmatched lines.")]
    BankAutoMatchResult,
    [Description("Bank statement line not found.")]
    BankStatementLineNotFound,
    [Description("Voucher line not found.")]
    VoucherLineNotFound,
    [Description("Matched successfully.")]
    BankLineMatched,
    [Description("Match removed.")]
    BankLineUnmatched,
    [Description("type must be 'Receivable' or 'Payable'.")]
    SubLedgerTypeInvalid,
    [Description("No control account found for {0}. Pass controlAccountId explicitly, or mark a Control Account whose name contains {1}")]
    SubLedgerControlAccountNotFound,

    // --- Reports / Budgets / Report Schedules ---
    [Description("format must be 'pdf' or 'excel'.")]
    ExportFormatInvalid,
    [Description("Unknown report type '{0}'.")]
    ReportTypeUnknown,
    [Description("A budget already exists for this account and period.")]
    BudgetAlreadyExists,
    [Description("Budget created successfully.")]
    BudgetCreated,
    [Description("Budget not found.")]
    BudgetNotFound,
    [Description("Budget updated successfully.")]
    BudgetUpdated,
    [Description("Budget deleted successfully.")]
    BudgetDeleted,
    [Description("Report schedule not found.")]
    ReportScheduleNotFound,
    [Description("Report schedule created successfully.")]
    ReportScheduleCreated,
    [Description("Report schedule updated successfully.")]
    ReportScheduleUpdated,
    [Description("Report schedule activated.")]
    ReportScheduleActivated,
    [Description("Report schedule deactivated.")]
    ReportScheduleDeactivated,
    [Description("Report schedule deleted successfully.")]
    ReportScheduleDeleted,

    // --- Auth ---
    [Description("Invalid username or password.")]
    InvalidCredentials,
    [Description("Login successful.")]
    LoginSuccessful,

    // --- Tenants ---
    [Description("Tenant not found.")]
    TenantNotFound,
    [Description("A tenant with this code already exists.")]
    TenantCodeExists,
    [Description("Tenant created successfully.")]
    TenantCreated,
    [Description("Tenant updated successfully.")]
    TenantUpdated,
    [Description("Tenant activated.")]
    TenantActivated,
    [Description("Tenant deactivated.")]
    TenantDeactivated,

    // --- Branches ---
    [Description("Branch not found.")]
    BranchNotFound,
    [Description("A branch with this code already exists for this tenant.")]
    BranchCodeExists,
    [Description("Branch created successfully.")]
    BranchCreated,
    [Description("Branch updated successfully.")]
    BranchUpdated,
    [Description("Branch activated.")]
    BranchActivated,
    [Description("Branch deactivated.")]
    BranchDeactivated,

    // --- Users (administration) ---
    [Description("User not found.")]
    AppUserNotFound,
    [Description("A user with this username already exists.")]
    UserUsernameExists,
    [Description("User created successfully.")]
    UserCreated,
    [Description("User updated successfully.")]
    UserUpdated,
    [Description("User activated.")]
    UserActivated,
    [Description("User deactivated.")]
    UserDeactivated,
    [Description("Only a system administrator can assign the SystemAdmin role.")]
    OnlySystemAdminCanAssignSystemAdmin,
    [Description("You do not have access to this tenant.")]
    TenantAccessDenied,
    [Description("Branch does not belong to this user's tenant.")]
    BranchTenantMismatch,
    [Description("Branch access granted.")]
    BranchAccessGranted,
    [Description("Branch access revoked.")]
    BranchAccessRevoked,

    // --- Roles / Rights ---
    [Description("Role not found.")]
    RoleNotFound,
    [Description("A role with this name already exists.")]
    RoleNameExists,
    [Description("The name of a built-in system role cannot be changed.")]
    RoleNameNotEditableForSystemRole,
    [Description("Built-in system roles cannot be deleted.")]
    RoleIsSystemRoleCannotDelete,
    [Description("This role is assigned to one or more users and cannot be deleted.")]
    RoleInUse,
    [Description("One or more selected rights do not belong to this role's tenant.")]
    RoleTenantMismatch,
    [Description("Role created successfully.")]
    RoleCreated,
    [Description("Role updated successfully.")]
    RoleUpdated,
    [Description("Role deleted successfully.")]
    RoleDeleted,
    [Description("One or more selected rights were not found.")]
    RightNotFound,

    // --- Product Categories ---
    [Description("Product category not found.")]
    ProductCategoryNotFound,
    [Description("Parent product category not found.")]
    ParentProductCategoryNotFound,
    [Description("A product category cannot be its own parent.")]
    ProductCategoryCannotBeOwnParent,
    [Description("This parent assignment would create a circular reference.")]
    ProductCategoryCircularReference,
    [Description("Product category created successfully.")]
    ProductCategoryCreated,
    [Description("Product category updated successfully.")]
    ProductCategoryUpdated,
    [Description("Product category activated.")]
    ProductCategoryActivated,
    [Description("Product category deactivated.")]
    ProductCategoryDeactivated,

    // --- Units of Measure ---
    [Description("Unit of measure not found.")]
    UnitOfMeasureNotFound,
    [Description("A unit of measure with this code already exists.")]
    UnitOfMeasureCodeExists,
    [Description("Unit of measure created successfully.")]
    UnitOfMeasureCreated,
    [Description("Unit of measure updated successfully.")]
    UnitOfMeasureUpdated,
    [Description("Unit of measure activated.")]
    UnitOfMeasureActivated,
    [Description("Unit of measure deactivated.")]
    UnitOfMeasureDeactivated,
    [Description("A UOM conversion for this unit already exists on this product.")]
    UOMConversionExists,
    [Description("UOM conversion not found.")]
    UOMConversionNotFound,
    [Description("Conversion factor must be greater than zero.")]
    UOMConversionFactorInvalid,
    [Description("The base unit of measure cannot also be added as a conversion.")]
    UOMConversionCannotBeBaseUnit,

    // --- Products / Variants / Pricing ---
    [Description("Product not found.")]
    ProductNotFound,
    [Description("A product with this SKU already exists.")]
    ProductSKUExists,
    [Description("Base unit of measure not found.")]
    BaseUnitOfMeasureNotFound,
    [Description("Product created successfully.")]
    ProductCreated,
    [Description("Product updated successfully.")]
    ProductUpdated,
    [Description("Product activated.")]
    ProductActivated,
    [Description("Product deactivated.")]
    ProductDeactivated,
    [Description("Product variant not found.")]
    ProductVariantNotFound,
    [Description("A variant with this code already exists for this product.")]
    ProductVariantCodeExists,
    [Description("A product must always have at least one active variant.")]
    ProductRequiresAtLeastOneVariant,
    [Description("Amount must not be negative.")]
    ProductVariantPriceAmountNegative,

    // --- Business Partners (Suppliers / Customers) ---
    [Description("Business partner not found.")]
    BusinessPartnerNotFound,
    [Description("A business partner with this code already exists.")]
    BusinessPartnerCodeExists,
    [Description("This partner is not a valid supplier.")]
    BusinessPartnerNotASupplier,
    [Description("This partner is not a valid customer.")]
    BusinessPartnerNotACustomer,
    [Description("Business partner created successfully.")]
    BusinessPartnerCreated,
    [Description("Business partner updated successfully.")]
    BusinessPartnerUpdated,
    [Description("Business partner activated.")]
    BusinessPartnerActivated,
    [Description("Business partner deactivated.")]
    BusinessPartnerDeactivated,

    // --- Warehouses ---
    [Description("Warehouse not found.")]
    WarehouseNotFound,
    [Description("A warehouse with this code already exists for this branch.")]
    WarehouseCodeExists,
    [Description("Warehouse created successfully.")]
    WarehouseCreated,
    [Description("Warehouse updated successfully.")]
    WarehouseUpdated,
    [Description("Warehouse activated.")]
    WarehouseActivated,
    [Description("Warehouse deactivated.")]
    WarehouseDeactivated,

    // --- Stock Account Mappings ---
    [Description("Stock account mapping not found.")]
    StockAccountMappingNotFound,
    [Description("A tenant-wide default mapping (no product category) already exists.")]
    StockAccountMappingDefaultAlreadyExists,
    [Description("A mapping for this product category already exists.")]
    StockAccountMappingCategoryAlreadyExists,
    [Description("One or more mapped accounts were not found.")]
    StockAccountMappingAccountNotFound,
    [Description("No applicable stock account mapping was found for this product's category, and no tenant-wide default is configured.")]
    StockAccountMappingNotConfigured,
    [Description("Stock account mapping created successfully.")]
    StockAccountMappingCreated,
    [Description("Stock account mapping updated successfully.")]
    StockAccountMappingUpdated,

    // --- Inventory / Stock Movement ---
    [Description("Insufficient stock: only {0} available at this warehouse.")]
    InsufficientStock,
    [Description("Stock balance not found for this product variant at this warehouse.")]
    StockBalanceNotFound,
    [Description("Warehouse not found.")]
    StockMovementWarehouseNotFound,
    [Description("Product variant not found.")]
    StockMovementVariantNotFound,

    // --- Invoices / Orders (all 6 InvoiceType values) ---
    [Description("Invoice not found.")]
    InvoiceNotFound,
    [Description("Partner not found.")]
    InvoicePartnerNotFound,
    [Description("Warehouse not found.")]
    InvoiceWarehouseNotFound,
    [Description("An invoice requires at least one line item.")]
    InvoiceRequiresAtLeastOneLine,
    [Description("Product variant {0} was not found.")]
    InvoiceLineVariantNotFound,
    [Description("Unit of measure {0} was not found.")]
    InvoiceLineUnitOfMeasureNotFound,
    [Description("Quantity must be greater than zero.")]
    InvoiceLineQuantityInvalid,
    [Description("Tax rate {0} was not found.")]
    InvoiceLineTaxRateNotFound,
    [Description("This document's date falls within a closed fiscal period.")]
    InvoiceDateInClosedPeriod,
    [Description("Only draft documents can be edited.")]
    InvoiceNotDraftForEdit,
    [Description("Only draft documents can be submitted for approval.")]
    InvoiceNotDraftForSubmit,
    [Description("Only documents pending approval can be approved.")]
    InvoiceNotPendingForApprove,
    [Description("Only documents pending approval can be rejected.")]
    InvoiceNotPendingForReject,
    [Description("This document's date falls within a closed fiscal period.")]
    InvoiceDateInClosedPeriodForApproval,
    [Description("Only draft, pending, or open/partially-fulfilled documents can be cancelled.")]
    InvoiceNotCancellable,
    [Description("A fully fulfilled order cannot be cancelled.")]
    InvoiceOrderFullyFulfilledCannotCancel,
    [Description("The referenced document was not found.")]
    InvoiceReferenceNotFound,
    [Description("The referenced document must be posted before this one can be raised against it.")]
    InvoiceReferenceNotPosted,
    [Description("The referenced document is not of the expected type for this operation.")]
    InvoiceReferenceTypeMismatch,
    [Description("A return requires a reference to the document being returned.")]
    InvoiceReturnRequiresReference,
    [Description("This referenced order line does not have enough remaining unfulfilled quantity.")]
    InvoiceReferenceLineOverFulfilled,
    [Description("No stock account mapping is configured for this transaction.")]
    InvoiceNoStockAccountMapping,
    [Description("Purchase order created as draft.")]
    PurchaseOrderCreated,
    [Description("Purchase order updated.")]
    PurchaseOrderUpdated,
    [Description("Purchase order submitted for approval.")]
    PurchaseOrderSubmitted,
    [Description("Purchase order approved.")]
    PurchaseOrderApproved,
    [Description("Purchase order rejected.")]
    PurchaseOrderRejected,
    [Description("Purchase order cancelled.")]
    PurchaseOrderCancelled,
    [Description("Sales order created as draft.")]
    SalesOrderCreated,
    [Description("Sales order updated.")]
    SalesOrderUpdated,
    [Description("Sales order submitted for approval.")]
    SalesOrderSubmitted,
    [Description("Sales order approved.")]
    SalesOrderApproved,
    [Description("Sales order rejected.")]
    SalesOrderRejected,
    [Description("Sales order cancelled.")]
    SalesOrderCancelled,
    [Description("Purchase invoice created as draft.")]
    PurchaseInvoiceCreated,
    [Description("Purchase invoice updated.")]
    PurchaseInvoiceUpdated,
    [Description("Purchase invoice submitted for approval.")]
    PurchaseInvoiceSubmitted,
    [Description("Purchase invoice approved and posted.")]
    PurchaseInvoiceApproved,
    [Description("Purchase invoice rejected.")]
    PurchaseInvoiceRejected,
    [Description("Sales invoice created as draft.")]
    SalesInvoiceCreated,
    [Description("Sales invoice updated.")]
    SalesInvoiceUpdated,
    [Description("Sales invoice submitted for approval.")]
    SalesInvoiceSubmitted,
    [Description("Sales invoice approved and posted.")]
    SalesInvoiceApproved,
    [Description("Sales invoice rejected.")]
    SalesInvoiceRejected,
    [Description("Purchase return created as draft.")]
    PurchaseReturnCreated,
    [Description("Purchase return updated.")]
    PurchaseReturnUpdated,
    [Description("Purchase return submitted for approval.")]
    PurchaseReturnSubmitted,
    [Description("Purchase return approved and posted.")]
    PurchaseReturnApproved,
    [Description("Purchase return rejected.")]
    PurchaseReturnRejected,
    [Description("Sale return created as draft.")]
    SaleReturnCreated,
    [Description("Sale return updated.")]
    SaleReturnUpdated,
    [Description("Sale return submitted for approval.")]
    SaleReturnSubmitted,
    [Description("Sale return approved and posted.")]
    SaleReturnApproved,
    [Description("Sale return rejected.")]
    SaleReturnRejected,

    // --- Partner Payments (Customer Receipts / Supplier Payments) ---
    [Description("Payment not found.")]
    PartnerPaymentNotFound,
    [Description("A payment requires at least one allocation.")]
    PartnerPaymentRequiresAtLeastOneAllocation,
    [Description("Allocated amount must be greater than zero.")]
    PartnerPaymentAllocationAmountInvalid,
    [Description("Allocation total ({0}) does not match the payment's total amount ({1}).")]
    PartnerPaymentAllocationTotalMismatch,
    [Description("This invoice does not belong to the selected partner.")]
    PartnerPaymentAllocationPartnerMismatch,
    [Description("This invoice type is not valid for this payment direction.")]
    PartnerPaymentAllocationInvoiceTypeMismatch,
    [Description("Allocated amount ({0}) exceeds this invoice's outstanding balance ({1}).")]
    PartnerPaymentAllocationExceedsOutstanding,
    [Description("This invoice is already fully paid.")]
    PartnerPaymentInvoiceAlreadyFullyPaid,
    [Description("Bank/cash account not found.")]
    PartnerPaymentBankAccountNotFound,
    [Description("Only draft payments can be edited.")]
    PartnerPaymentNotDraftForEdit,
    [Description("Only draft payments can be submitted for approval.")]
    PartnerPaymentNotDraftForSubmit,
    [Description("Only payments pending approval can be approved.")]
    PartnerPaymentNotPendingForApprove,
    [Description("Only payments pending approval can be rejected.")]
    PartnerPaymentNotPendingForReject,
    [Description("Customer receipt created as draft.")]
    CustomerReceiptCreated,
    [Description("Customer receipt submitted for approval.")]
    CustomerReceiptSubmitted,
    [Description("Customer receipt approved and posted.")]
    CustomerReceiptApproved,
    [Description("Customer receipt rejected.")]
    CustomerReceiptRejected,
    [Description("Supplier payment created as draft.")]
    SupplierPaymentCreated,
    [Description("Supplier payment submitted for approval.")]
    SupplierPaymentSubmitted,
    [Description("Supplier payment approved and posted.")]
    SupplierPaymentApproved,
    [Description("Supplier payment rejected.")]
    SupplierPaymentRejected,

    // --- Stock Transfers ---
    [Description("Stock transfer not found.")]
    StockTransferNotFound,
    [Description("Source and destination warehouse must be different.")]
    StockTransferSameWarehouse,
    [Description("A stock transfer requires at least one line item.")]
    StockTransferRequiresAtLeastOneLine,
    [Description("Only draft transfers can be edited.")]
    StockTransferNotDraftForEdit,
    [Description("Only draft transfers can be submitted for approval.")]
    StockTransferNotDraftForSubmit,
    [Description("Only transfers pending approval can be approved.")]
    StockTransferNotPendingForApprove,
    [Description("Only transfers pending approval can be rejected.")]
    StockTransferNotPendingForReject,
    [Description("Only transfers awaiting receipt can be received.")]
    StockTransferNotPendingReceipt,
    [Description("Received quantity cannot exceed the transferred quantity.")]
    StockTransferReceivedQtyExceedsTransferred,
    [Description("This document's date falls within a closed fiscal period.")]
    StockTransferDateInClosedPeriod,
    [Description("Stock transfer created as draft.")]
    StockTransferCreated,
    [Description("Stock transfer updated.")]
    StockTransferUpdated,
    [Description("Stock transfer submitted for approval.")]
    StockTransferSubmitted,
    [Description("Stock transfer approved.")]
    StockTransferApproved,
    [Description("Stock transfer rejected.")]
    StockTransferRejected,
    [Description("Stock transfer receipt recorded.")]
    StockTransferReceived,

    // --- Stock Adjustments ---
    [Description("Stock adjustment not found.")]
    StockAdjustmentNotFound,
    [Description("A stock adjustment requires at least one line item.")]
    StockAdjustmentRequiresAtLeastOneLine,
    [Description("Unit cost is required for an Increase line.")]
    StockAdjustmentUnitCostRequiredForIncrease,
    [Description("An opening balance line can only be posted once per product variant and warehouse.")]
    StockAdjustmentOpeningBalanceAlreadyExists,
    [Description("Only draft adjustments can be edited.")]
    StockAdjustmentNotDraftForEdit,
    [Description("Only draft adjustments can be submitted for approval.")]
    StockAdjustmentNotDraftForSubmit,
    [Description("Only adjustments pending approval can be approved.")]
    StockAdjustmentNotPendingForApprove,
    [Description("Only adjustments pending approval can be rejected.")]
    StockAdjustmentNotPendingForReject,
    [Description("This document's date falls within a closed fiscal period.")]
    StockAdjustmentDateInClosedPeriod,
    [Description("Stock adjustment created as draft.")]
    StockAdjustmentCreated,
    [Description("Stock adjustment updated.")]
    StockAdjustmentUpdated,
    [Description("Stock adjustment submitted for approval.")]
    StockAdjustmentSubmitted,
    [Description("Stock adjustment approved and posted.")]
    StockAdjustmentApproved,
    [Description("Stock adjustment rejected.")]
    StockAdjustmentRejected,
}

public static class ResponseMessageExtensions
{
    public static string ToText(this ResponseMessage message, params object[] args)
    {
        var field = typeof(ResponseMessage).GetField(message.ToString());
        var text = field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? message.ToString();
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}
