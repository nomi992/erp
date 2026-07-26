using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.UnitsOfMeasure.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.UnitsOfMeasure;

public class UnitOfMeasureRepository : IUnitOfMeasureRepository
{
    private readonly AppDbContext _context;

    public UnitOfMeasureRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UnitOfMeasureResponse>> GetAllAsync() =>
        await _context.UnitsOfMeasure
            .OrderBy(u => u.Name)
            .Select(u => UnitOfMeasureResponse.FromEntity(u))
            .ToListAsync();

    public async Task<UnitOfMeasureResponse> GetByIdAsync(int id)
    {
        var uom = await _context.UnitsOfMeasure.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.UnitOfMeasureNotFound);
        return UnitOfMeasureResponse.FromEntity(uom);
    }

    public async Task<UnitOfMeasureResponse> CreateAsync(UnitOfMeasureRequest request)
    {
        if (await _context.UnitsOfMeasure.AnyAsync(u => u.Code == request.Code))
        {
            throw new BadRequestException(ResponseMessage.UnitOfMeasureCodeExists);
        }

        var uom = new UnitOfMeasure { Code = request.Code, Name = request.Name };
        _context.UnitsOfMeasure.Add(uom);
        await _context.SaveChangesAsync();

        return UnitOfMeasureResponse.FromEntity(uom);
    }

    public async Task<UnitOfMeasureResponse> UpdateAsync(int id, UnitOfMeasureRequest request)
    {
        var uom = await _context.UnitsOfMeasure.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.UnitOfMeasureNotFound);

        if (await _context.UnitsOfMeasure.AnyAsync(u => u.Code == request.Code && u.Id != id))
        {
            throw new BadRequestException(ResponseMessage.UnitOfMeasureCodeExists);
        }

        uom.Code = request.Code;
        uom.Name = request.Name;
        await _context.SaveChangesAsync();

        return UnitOfMeasureResponse.FromEntity(uom);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var uom = await _context.UnitsOfMeasure.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.UnitOfMeasureNotFound);
        uom.IsActive = isActive;
        await _context.SaveChangesAsync();
    }
}
