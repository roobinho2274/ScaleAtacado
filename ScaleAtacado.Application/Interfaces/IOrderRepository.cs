using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, Guid companyId);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetAllAsync(
        Guid companyId, int page, int pageSize,
        Guid? customerId, DeliveryStatus? deliveryStatus, FinancialStatus? financialStatus,
        DateTime? from, DateTime? to);
    Task<int> GetNextOrderNumberAsync(Guid companyId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> SaveChangesAsync();
}
