using erp_backend.Auth;
using erp_backend.Branches.Dtos;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Branches;

public class BranchRepository : IBranchRepository
{
    private readonly AppDbContext _context;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchRepository(AppDbContext context, ICurrentTenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsSystemAdmin => _httpContextAccessor.HttpContext?.User.IsInRole(AppRoles.SystemAdmin) == true;

    public async Task<List<BranchResponse>> GetAllAsync(int? tenantId)
    {
        var query = _context.Branches.Include(b => b.Tenant).AsQueryable();

        query = IsSystemAdmin
            ? (tenantId is int t ? query.Where(b => b.TenantId == t) : query)
            : query.Where(b => b.TenantId == _tenantContext.TenantId);

        var branches = await query.OrderBy(b => b.Name).ToListAsync();
        return branches.Select(b => BranchResponse.FromEntity(b)).ToList();
    }

    public async Task<BranchResponse> GetByIdAsync(int id)
    {
        var branch = await GetOwnedBranchAsync(id);
        return BranchResponse.FromEntity(branch);
    }

    public async Task<BranchResponse> CreateAsync(CreateBranchRequest request)
    {
        var targetTenantId = IsSystemAdmin && request.TenantId is int t ? t : _tenantContext.TenantId;

        var tenant = await _context.Tenants.FirstOrDefaultAsync(x => x.Id == targetTenantId)
            ?? throw new NotFoundException(ResponseMessage.TenantNotFound);

        var codeExists = await _context.Branches.AnyAsync(b => b.TenantId == targetTenantId && b.Code == request.Code);
        if (codeExists)
        {
            throw new BadRequestException(ResponseMessage.BranchCodeExists);
        }

        var branch = new Branch { TenantId = targetTenantId, Name = request.Name, Code = request.Code };
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return BranchResponse.FromEntity(branch, tenant.Name);
    }

    public async Task<BranchResponse> UpdateAsync(int id, UpdateBranchRequest request)
    {
        var branch = await GetOwnedBranchAsync(id);

        var codeExists = await _context.Branches.AnyAsync(b => b.TenantId == branch.TenantId && b.Code == request.Code && b.Id != id);
        if (codeExists)
        {
            throw new BadRequestException(ResponseMessage.BranchCodeExists);
        }

        branch.Name = request.Name;
        branch.Code = request.Code;
        await _context.SaveChangesAsync();

        return BranchResponse.FromEntity(branch);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var branch = await GetOwnedBranchAsync(id);
        branch.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private async Task<Branch> GetOwnedBranchAsync(int id)
    {
        var branch = await _context.Branches.Include(b => b.Tenant).FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException(ResponseMessage.BranchNotFound);

        if (!IsSystemAdmin && branch.TenantId != _tenantContext.TenantId)
        {
            // Behave as if it doesn't exist rather than confirming another tenant's branch ID.
            throw new NotFoundException(ResponseMessage.BranchNotFound);
        }

        return branch;
    }
}
