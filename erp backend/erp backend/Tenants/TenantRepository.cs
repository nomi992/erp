using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Tenants.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Tenants;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantResponse>> GetAllAsync()
    {
        var tenants = await _context.Tenants.OrderBy(t => t.Name).ToListAsync();
        return await ToResponsesAsync(tenants);
    }

    public async Task<TenantResponse> GetByIdAsync(int id)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(ResponseMessage.TenantNotFound);

        var responses = await ToResponsesAsync([tenant]);
        return responses[0];
    }

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request)
    {
        var codeExists = await _context.Tenants.AnyAsync(t => t.Code == request.Code);
        if (codeExists)
        {
            throw new BadRequestException(ResponseMessage.TenantCodeExists);
        }

        var tenant = new Tenant { Name = request.Name, Code = request.Code };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return TenantResponse.FromEntity(tenant);
    }

    public async Task<TenantResponse> UpdateAsync(int id, UpdateTenantRequest request)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(ResponseMessage.TenantNotFound);

        tenant.Name = request.Name;
        await _context.SaveChangesAsync();

        var responses = await ToResponsesAsync([tenant]);
        return responses[0];
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(ResponseMessage.TenantNotFound);

        tenant.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private async Task<List<TenantResponse>> ToResponsesAsync(List<Tenant> tenants)
    {
        var tenantIds = tenants.Select(t => t.Id).ToList();

        var branchCounts = await _context.Branches
            .Where(b => tenantIds.Contains(b.TenantId))
            .GroupBy(b => b.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.TenantId, g => g.Count);

        var userCounts = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => tenantIds.Contains(u.TenantId))
            .GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.TenantId, g => g.Count);

        return tenants
            .Select(t => TenantResponse.FromEntity(
                t,
                branchCounts.GetValueOrDefault(t.Id),
                userCounts.GetValueOrDefault(t.Id)))
            .ToList();
    }
}
