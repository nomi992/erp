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
