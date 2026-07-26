using erp_backend.Accounts.Dtos;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Repositories;

namespace erp_backend.Accounts;

public class AccountRepository : IAccountRepository
{
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<VoucherDetail> _voucherDetails;

    public AccountRepository(IRepository<Account> accounts, IRepository<VoucherDetail> voucherDetails)
    {
        _accounts = accounts;
        _voucherDetails = voucherDetails;
    }

    public async Task<List<AccountResponse>> GetAllAsync(bool? isActive)
    {
        var accounts = await _accounts.GetAllAsync();
        var accountsById = accounts.ToDictionary(a => a.Id);

        var filtered = isActive is null
            ? accounts
            : accounts.Where(a => a.IsActive == isActive).ToList();

        return filtered
            .Select(a => AccountResponse.FromEntity(
                a,
                a.ParentAccountId is int parentId && accountsById.TryGetValue(parentId, out var parent) ? parent.Name : null))
            .OrderBy(a => a.Code)
            .ToList();
    }

    public async Task<List<AccountTreeNodeResponse>> GetTreeAsync()
    {
        var accounts = await _accounts.GetAllAsync();

        var nodesById = accounts.ToDictionary(a => a.Id, AccountTreeNodeResponse.FromEntity);
        var roots = new List<AccountTreeNodeResponse>();

        foreach (var account in accounts.OrderBy(a => a.Code))
        {
            var node = nodesById[account.Id];

            if (account.ParentAccountId is int parentId && nodesById.TryGetValue(parentId, out var parentNode))
            {
                parentNode.Children.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    public async Task<AccountResponse> GetByIdAsync(int id)
    {
        var account = await _accounts.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.AccountNotFound);

        string? parentName = null;
        if (account.ParentAccountId is int parentId)
        {
            parentName = (await _accounts.GetByIdAsync(parentId))?.Name;
        }

        return AccountResponse.FromEntity(account, parentName);
    }

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request)
    {
        var codeTaken = (await _accounts.FindAsync(a => a.Code == request.Code)).Count > 0;
        if (codeTaken)
        {
            throw new BadRequestException(ResponseMessage.AccountCodeExists);
        }

        Account? parent = null;
        if (request.ParentAccountId is int parentId)
        {
            parent = await _accounts.GetByIdAsync(parentId) ?? throw new BadRequestException(ResponseMessage.ParentAccountNotFound);
        }

        var account = new Account
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Nature = request.Nature,
            ParentAccountId = request.ParentAccountId,
            IsControlAccount = request.IsControlAccount,
            IsCashAccount = request.IsCashAccount,
            OpeningBalance = request.OpeningBalance,
            OpeningBalanceNature = request.OpeningBalanceNature,
        };

        await _accounts.AddAsync(account);
        await _accounts.SaveChangesAsync();

        return AccountResponse.FromEntity(account, parent?.Name);
    }

    public async Task<AccountResponse> UpdateAsync(int id, UpdateAccountRequest request)
    {
        var account = await _accounts.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.AccountNotFound);

        var codeTaken = (await _accounts.FindAsync(a => a.Code == request.Code && a.Id != id)).Count > 0;
        if (codeTaken)
        {
            throw new BadRequestException(ResponseMessage.AccountCodeExists);
        }

        Account? parent = null;
        if (request.ParentAccountId is int parentId)
        {
            if (parentId == id)
            {
                throw new BadRequestException(ResponseMessage.AccountCannotBeOwnParent);
            }

            parent = await _accounts.GetByIdAsync(parentId) ?? throw new BadRequestException(ResponseMessage.ParentAccountNotFound);

            if (await CreatesCircularReferenceAsync(id, parentId))
            {
                throw new BadRequestException(ResponseMessage.CircularReference);
            }
        }

        account.Code = request.Code;
        account.Name = request.Name;
        account.Type = request.Type;
        account.Nature = request.Nature;
        account.ParentAccountId = request.ParentAccountId;
        account.IsControlAccount = request.IsControlAccount;
        account.IsCashAccount = request.IsCashAccount;
        account.OpeningBalance = request.OpeningBalance;
        account.OpeningBalanceNature = request.OpeningBalanceNature;

        _accounts.Update(account);
        await _accounts.SaveChangesAsync();

        return AccountResponse.FromEntity(account, parent?.Name);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    public async Task DeleteAsync(int id)
    {
        var account = await _accounts.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.AccountNotFound);

        var hasChildren = (await _accounts.FindAsync(a => a.ParentAccountId == id)).Count > 0;
        if (hasChildren)
        {
            throw new BadRequestException(ResponseMessage.AccountHasChildren);
        }

        var hasTransactions = (await _voucherDetails.FindAsync(d => d.AccountId == id)).Count > 0;
        if (hasTransactions)
        {
            throw new BadRequestException(ResponseMessage.AccountHasTransactions);
        }

        _accounts.Remove(account);
        await _accounts.SaveChangesAsync();
    }

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var account = await _accounts.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.AccountNotFound);

        account.IsActive = isActive;
        _accounts.Update(account);
        await _accounts.SaveChangesAsync();
    }

    private async Task<bool> CreatesCircularReferenceAsync(int accountId, int proposedParentId)
    {
        var currentId = (int?)proposedParentId;
        var visited = new HashSet<int>();

        while (currentId is int id)
        {
            if (id == accountId)
            {
                return true;
            }

            if (!visited.Add(id))
            {
                break;
            }

            var current = await _accounts.GetByIdAsync(id);
            currentId = current?.ParentAccountId;
        }

        return false;
    }
}
