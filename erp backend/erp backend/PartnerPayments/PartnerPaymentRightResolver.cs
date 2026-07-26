using erp_backend.Auth;
using erp_backend.Models;

namespace erp_backend.PartnerPayments;

public enum PaymentAction
{
    Create,
    Submit,
    Approve,
    Reject,
}

public static class PartnerPaymentRightResolver
{
    public static string Resolve(PaymentDirection direction, PaymentAction action) => (direction, action) switch
    {
        (PaymentDirection.CustomerReceipt, PaymentAction.Create) => RightCodes.CustomerReceiptsCreate,
        (PaymentDirection.CustomerReceipt, PaymentAction.Submit) => RightCodes.CustomerReceiptsSubmit,
        (PaymentDirection.CustomerReceipt, PaymentAction.Approve) => RightCodes.CustomerReceiptsApprove,
        (PaymentDirection.CustomerReceipt, PaymentAction.Reject) => RightCodes.CustomerReceiptsReject,

        (PaymentDirection.SupplierPayment, PaymentAction.Create) => RightCodes.SupplierPaymentsCreate,
        (PaymentDirection.SupplierPayment, PaymentAction.Submit) => RightCodes.SupplierPaymentsSubmit,
        (PaymentDirection.SupplierPayment, PaymentAction.Approve) => RightCodes.SupplierPaymentsApprove,
        (PaymentDirection.SupplierPayment, PaymentAction.Reject) => RightCodes.SupplierPaymentsReject,

        _ => throw new InvalidOperationException($"No right is defined for {direction}/{action}."),
    };
}
