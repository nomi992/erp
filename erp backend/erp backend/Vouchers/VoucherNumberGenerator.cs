using erp_backend.Data;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Vouchers;

public static class VoucherNumberGenerator
{
    private static readonly Dictionary<VoucherType, string> Prefixes = new()
    {
        [VoucherType.Payment] = "PMT",
        [VoucherType.Receipt] = "RCT",
        [VoucherType.Journal] = "JV",
        [VoucherType.Sales] = "SAL",
        [VoucherType.Purchase] = "PUR",
        [VoucherType.Contra] = "CTR",
        [VoucherType.DebitNote] = "DN",
        [VoucherType.CreditNote] = "CN",
    };

    public static async Task<string> GenerateAsync(AppDbContext context, VoucherType type, DateTime date)
    {
        var year = date.Year;
        var count = await context.VoucherHeaders.CountAsync(v => v.VoucherType == type && v.Date.Year == year);
        return $"{Prefixes[type]}-{year}-{count + 1:0000}";
    }
}
