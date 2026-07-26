using erp_backend.Models;

namespace erp_backend.Vouchers.Dtos;

public class RecurringTemplateLineResponse
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
}

public class RecurringVoucherTemplateResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public string NarrationTemplate { get; set; } = string.Empty;
    public RecurringFrequency Frequency { get; set; }
    public DateTime NextRunDate { get; set; }
    public bool IsActive { get; set; }
    public List<RecurringTemplateLineResponse> Lines { get; set; } = [];

    public static RecurringVoucherTemplateResponse FromEntity(RecurringVoucherTemplate template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        VoucherType = template.VoucherType,
        NarrationTemplate = template.NarrationTemplate,
        Frequency = template.Frequency,
        NextRunDate = template.NextRunDate,
        IsActive = template.IsActive,
        Lines = template.Lines.Select(l => new RecurringTemplateLineResponse
        {
            Id = l.Id,
            AccountId = l.AccountId,
            AccountCode = l.Account?.Code ?? string.Empty,
            AccountName = l.Account?.Name ?? string.Empty,
            DebitAmount = l.DebitAmount,
            CreditAmount = l.CreditAmount,
            CostCenterId = l.CostCenterId,
            CostCenterName = l.CostCenter?.Name,
        }).ToList(),
    };
}
