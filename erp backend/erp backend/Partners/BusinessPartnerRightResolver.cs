using erp_backend.Auth;
using erp_backend.Models;

namespace erp_backend.Partners;

public enum PartnerAction
{
    Create,
    Edit,
}

/// <summary>
/// BusinessPartner is one shared table/API for Suppliers and Customers, but rights stay granular per
/// side. A PartnerType.Both partner needs BOTH the Supplier-side and Customer-side right at once —
/// a user must be trusted to manage both sides to create or edit a dual-role record.
/// </summary>
public static class BusinessPartnerRightResolver
{
    public static IReadOnlyList<string> Resolve(PartnerType partnerType, PartnerAction action)
    {
        var codes = new List<string>();

        if (partnerType is PartnerType.Supplier or PartnerType.Both)
        {
            codes.Add(action == PartnerAction.Create ? RightCodes.SuppliersCreate : RightCodes.SuppliersEdit);
        }

        if (partnerType is PartnerType.Customer or PartnerType.Both)
        {
            codes.Add(action == PartnerAction.Create ? RightCodes.CustomersCreate : RightCodes.CustomersEdit);
        }

        return codes;
    }
}
