using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class VoucherDetail : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int VoucherId { get; set; }
    public VoucherHeader? Voucher { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public int? TaxRateId { get; set; }
    public TaxRate? TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}
