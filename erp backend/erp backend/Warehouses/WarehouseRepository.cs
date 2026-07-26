using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Warehouses.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Warehouses;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _context;

    public WarehouseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseResponse>> GetAllAsync()
    {
        var warehouses = await _context.Warehouses.Include(w => w.CostCenter).OrderBy(w => w.Name).ToListAsync();
        return warehouses.Select(WarehouseResponse.FromEntity).ToList();
    }

    public async Task<WarehouseResponse> GetByIdAsync(int id)
    {
        var warehouse = await _context.Warehouses.Include(w => w.CostCenter).FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new NotFoundException(ResponseMessage.WarehouseNotFound);

        return WarehouseResponse.FromEntity(warehouse);
    }

    public async Task<WarehouseResponse> CreateAsync(WarehouseRequest request)
    {
        if (await _context.Warehouses.AnyAsync(w => w.Code == request.Code))
        {
            throw new BadRequestException(ResponseMessage.WarehouseCodeExists);
        }

        if (request.CostCenterId is int costCenterId && await _context.CostCenters.FindAsync(costCenterId) is null)
        {
            throw new BadRequestException(ResponseMessage.CostCenterNotFound);
        }

        var warehouse = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            CostCenterId = request.CostCenterId,
            IsDefault = request.IsDefault,
        };

        if (request.IsDefault)
        {
            await ClearOtherDefaultsAsync(null);
        }

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(warehouse.Id);
    }

    public async Task<WarehouseResponse> UpdateAsync(int id, WarehouseRequest request)
    {
        var warehouse = await _context.Warehouses.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.WarehouseNotFound);

        if (await _context.Warehouses.AnyAsync(w => w.Code == request.Code && w.Id != id))
        {
            throw new BadRequestException(ResponseMessage.WarehouseCodeExists);
        }

        if (request.CostCenterId is int costCenterId && await _context.CostCenters.FindAsync(costCenterId) is null)
        {
            throw new BadRequestException(ResponseMessage.CostCenterNotFound);
        }

        warehouse.Code = request.Code;
        warehouse.Name = request.Name;
        warehouse.CostCenterId = request.CostCenterId;

        if (request.IsDefault && !warehouse.IsDefault)
        {
            await ClearOtherDefaultsAsync(id);
        }

        warehouse.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var warehouse = await _context.Warehouses.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.WarehouseNotFound);
        warehouse.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private async Task ClearOtherDefaultsAsync(int? exceptId)
    {
        var others = await _context.Warehouses.Where(w => w.IsDefault && w.Id != (exceptId ?? -1)).ToListAsync();
        foreach (var other in others)
        {
            other.IsDefault = false;
        }
    }
}
