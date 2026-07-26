using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Vouchers.Dtos;

public class CreateVoucherRequest
{
    [Required]
    public VoucherType VoucherType { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public string Narration { get; set; } = string.Empty;

    public string CurrencyCode { get; set; } = "USD";

    public decimal ExchangeRate { get; set; } = 1;

    [MinLength(2, ErrorMessage = "A voucher requires at least two line items.")]
    public List<VoucherLineRequest> Lines { get; set; } = [];
}
