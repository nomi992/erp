using System.ComponentModel.DataAnnotations;

namespace erp_backend.Vouchers.Dtos;

public class VoucherLineRequest
{
    [Required]
    public int AccountId { get; set; }

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? CostCenterId { get; set; }
    public int? TaxRateId { get; set; }
}
