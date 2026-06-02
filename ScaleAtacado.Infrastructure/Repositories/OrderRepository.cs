using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, Guid companyId)
        => await _context.Orders
            .Include(o => o.Cliente)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllAsync(
        Guid companyId, int page, int pageSize,
        Guid? clienteId, DeliveryStatus? deliveryStatus, FinancialStatus? financialStatus,
        DateTime? from, DateTime? to)
    {
        var query = _context.Orders
            .Include(o => o.Cliente)
            .Include(o => o.PaymentMethod)
            .Where(o => o.CompanyId == companyId);

        if (clienteId.HasValue)
            query = query.Where(o => o.ClienteId == clienteId.Value);

        if (deliveryStatus.HasValue)
            query = query.Where(o => o.DeliveryStatus == deliveryStatus.Value);

        if (financialStatus.HasValue)
            query = query.Where(o => o.FinancialStatus == financialStatus.Value);

        if (from.HasValue)
            query = query.Where(o => o.OrderDate >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.OrderDate <= to.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.OrderNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> GetNextOrderNumberAsync(Guid companyId)
    {
        var max = await _context.Orders
            .Where(o => o.CompanyId == companyId)
            .MaxAsync(o => (int?)o.OrderNumber) ?? 0;
        return max + 1;
    }

    public async Task AddAsync(Order order)
        => await _context.Orders.AddAsync(order);

    public Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
