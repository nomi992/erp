using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Vouchers.Dtos;

public class RecurringTemplateLineRequest
{
    [Required]
    public int AccountId { get; set; }

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? CostCenterId { get; set; }
}

public class RecurringVoucherTemplateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public VoucherType VoucherType { get; set; }

    public string NarrationTemplate { get; set; } = string.Empty;

    [Required]
    public RecurringFrequency Frequency { get; set; }

    [Required]
    public DateTime NextRunDate { get; set; }

    [MinLength(2, ErrorMessage = "A template requires at least two line items.")]
    public List<RecurringTemplateLineRequest> Lines { get; set; } = [];
}
