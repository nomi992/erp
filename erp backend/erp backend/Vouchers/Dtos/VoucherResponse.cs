using erp_backend.Models;

namespace erp_backend.Vouchers.Dtos;

public class VoucherLineResponse
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public int? TaxRateId { get; set; }
    public string? TaxRateName { get; set; }
    public decimal TaxAmount { get; set; }
}

public class VoucherAttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

public class VoucherResponse
{
    public int Id { get; set; }
    public VoucherType VoucherType { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public VoucherStatus Status { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public int? ReversalOfVoucherId { get; set; }
    public string? ReversalOfVoucherNo { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<VoucherLineResponse> Lines { get; set; } = [];
    public List<VoucherAttachmentResponse> Attachments { get; set; } = [];
}

public class VoucherListItemResponse
{
    public int Id { get; set; }
    public VoucherType VoucherType { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public VoucherStatus Status { get; set; }
    public decimal TotalDebit { get; set; }
}
