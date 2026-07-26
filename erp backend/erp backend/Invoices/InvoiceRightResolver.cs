using erp_backend.Auth;
using erp_backend.Models;

namespace erp_backend.Invoices;

public enum InvoiceAction
{
    Create,
    Edit,
    Submit,
    Approve,
    Reject,
    Cancel,
}

/// <summary>
/// Invoices/Orders share one table and one controller, but rights stay fully granular per business
/// document type + action. Which right applies depends on the row's InvoiceType, which a static
/// [Authorize(Policy=...)] attribute can't express — so the controller resolves the concrete right in
/// code and checks it programmatically via IAuthorizationService instead.
/// </summary>
public static class InvoiceRightResolver
{
    public static string Resolve(InvoiceType invoiceType, InvoiceAction action) => (invoiceType, action) switch
    {
        (InvoiceType.PurchaseOrder, InvoiceAction.Create) => RightCodes.PurchaseOrdersCreate,
        (InvoiceType.PurchaseOrder, InvoiceAction.Edit) => RightCodes.PurchaseOrdersEdit,
        (InvoiceType.PurchaseOrder, InvoiceAction.Submit) => RightCodes.PurchaseOrdersSubmit,
        (InvoiceType.PurchaseOrder, InvoiceAction.Approve) => RightCodes.PurchaseOrdersApprove,
        (InvoiceType.PurchaseOrder, InvoiceAction.Reject) => RightCodes.PurchaseOrdersReject,
        (InvoiceType.PurchaseOrder, InvoiceAction.Cancel) => RightCodes.PurchaseOrdersCancel,

        (InvoiceType.SalesOrder, InvoiceAction.Create) => RightCodes.SalesOrdersCreate,
        (InvoiceType.SalesOrder, InvoiceAction.Edit) => RightCodes.SalesOrdersEdit,
        (InvoiceType.SalesOrder, InvoiceAction.Submit) => RightCodes.SalesOrdersSubmit,
        (InvoiceType.SalesOrder, InvoiceAction.Approve) => RightCodes.SalesOrdersApprove,
        (InvoiceType.SalesOrder, InvoiceAction.Reject) => RightCodes.SalesOrdersReject,
        (InvoiceType.SalesOrder, InvoiceAction.Cancel) => RightCodes.SalesOrdersCancel,

        (InvoiceType.PurchaseInvoice, InvoiceAction.Create) => RightCodes.PurchaseInvoicesCreate,
        (InvoiceType.PurchaseInvoice, InvoiceAction.Edit) => RightCodes.PurchaseInvoicesEdit,
        (InvoiceType.PurchaseInvoice, InvoiceAction.Submit) => RightCodes.PurchaseInvoicesSubmit,
        (InvoiceType.PurchaseInvoice, InvoiceAction.Approve) => RightCodes.PurchaseInvoicesApprove,
        (InvoiceType.PurchaseInvoice, InvoiceAction.Reject) => RightCodes.PurchaseInvoicesReject,
        // No dedicated Cancel right exists for non-order types — cancelling an unposted draft/pending
        // document requires the same authority as rejecting it.
        (InvoiceType.PurchaseInvoice, InvoiceAction.Cancel) => RightCodes.PurchaseInvoicesReject,

        (InvoiceType.SalesInvoice, InvoiceAction.Create) => RightCodes.SalesInvoicesCreate,
        (InvoiceType.SalesInvoice, InvoiceAction.Edit) => RightCodes.SalesInvoicesEdit,
        (InvoiceType.SalesInvoice, InvoiceAction.Submit) => RightCodes.SalesInvoicesSubmit,
        (InvoiceType.SalesInvoice, InvoiceAction.Approve) => RightCodes.SalesInvoicesApprove,
        (InvoiceType.SalesInvoice, InvoiceAction.Reject) => RightCodes.SalesInvoicesReject,
        (InvoiceType.SalesInvoice, InvoiceAction.Cancel) => RightCodes.SalesInvoicesReject,

        (InvoiceType.PurchaseReturn, InvoiceAction.Create) => RightCodes.PurchaseReturnsCreate,
        (InvoiceType.PurchaseReturn, InvoiceAction.Edit) => RightCodes.PurchaseReturnsEdit,
        (InvoiceType.PurchaseReturn, InvoiceAction.Submit) => RightCodes.PurchaseReturnsSubmit,
        (InvoiceType.PurchaseReturn, InvoiceAction.Approve) => RightCodes.PurchaseReturnsApprove,
        (InvoiceType.PurchaseReturn, InvoiceAction.Reject) => RightCodes.PurchaseReturnsReject,
        (InvoiceType.PurchaseReturn, InvoiceAction.Cancel) => RightCodes.PurchaseReturnsReject,

        (InvoiceType.SaleReturn, InvoiceAction.Create) => RightCodes.SaleReturnsCreate,
        (InvoiceType.SaleReturn, InvoiceAction.Edit) => RightCodes.SaleReturnsEdit,
        (InvoiceType.SaleReturn, InvoiceAction.Submit) => RightCodes.SaleReturnsSubmit,
        (InvoiceType.SaleReturn, InvoiceAction.Approve) => RightCodes.SaleReturnsApprove,
        (InvoiceType.SaleReturn, InvoiceAction.Reject) => RightCodes.SaleReturnsReject,
        (InvoiceType.SaleReturn, InvoiceAction.Cancel) => RightCodes.SaleReturnsReject,

        _ => throw new InvalidOperationException($"No right is defined for {invoiceType}/{action}."),
    };
}
