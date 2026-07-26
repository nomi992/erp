using erp_backend.Models;

namespace erp_backend.Ledgers;

public static class BalanceMath
{
    /// <summary>Converts a debit/credit pair into a value signed positive in the account's natural direction.</summary>
    public static decimal SignedDelta(AccountNature accountNature, decimal debit, decimal credit) =>
        accountNature == AccountNature.Debit ? debit - credit : credit - debit;

    public static decimal SignedOpeningBalance(Account account) =>
        account.OpeningBalanceNature == account.Nature ? account.OpeningBalance : -account.OpeningBalance;

    public static (decimal Amount, AccountNature Nature) ToDisplayBalance(decimal signedAmount, AccountNature accountNature) =>
        signedAmount >= 0
            ? (signedAmount, accountNature)
            : (-signedAmount, accountNature == AccountNature.Debit ? AccountNature.Credit : AccountNature.Debit);
}
