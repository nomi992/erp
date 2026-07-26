using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum RecurringFrequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly,
}

public class RecurringVoucherTemplate : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public string NarrationTemplate { get; set; } = string.Empty;
    public RecurringFrequency Frequency { get; set; }
    public DateTime NextRunDate { get; set; }
    public bool IsActive { get; set; } = true;

    public List<RecurringVoucherTemplateLine> Lines { get; set; } = [];
}

public class RecurringVoucherTemplateLine : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int RecurringVoucherTemplateId { get; set; }
    public RecurringVoucherTemplate? RecurringVoucherTemplate { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
}
