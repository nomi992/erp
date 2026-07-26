using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Ledgers.Dtos;

public class BankStatementLineResponse
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public bool IsMatched { get; set; }
    public int? MatchedVoucherDetailId { get; set; }
    public int? MatchedVoucherId { get; set; }
    public string? MatchedVoucherNo { get; set; }

    public static BankStatementLineResponse FromEntity(BankStatementLine line) => new()
    {
        Id = line.Id,
        TransactionDate = line.TransactionDate,
        Amount = line.Amount,
        Description = line.Description,
        ReferenceNo = line.ReferenceNo,
        IsMatched = line.IsMatched,
        MatchedVoucherDetailId = line.MatchedVoucherDetailId,
        MatchedVoucherId = line.MatchedVoucherDetail?.VoucherId,
        MatchedVoucherNo = line.MatchedVoucherDetail?.Voucher?.VoucherNo,
    };
}

public class ManualMatchRequest
{
    [Required]
    public int BankStatementLineId { get; set; }

    [Required]
    public int VoucherDetailId { get; set; }
}
