using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, Guid companyId);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetAllAsync(
        Guid companyId, int page, int pageSize,
        Guid? customerId, OrderStatus? orderStatus, FinancialStatus? financialStatus,
        DateTime? from, DateTime? to);
    Task<int> GetNextOrderNumberAsync(Guid companyId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task ReplaceItemsAsync(Order order, List<OrderItem> newItems);
    Task ReplacePaymentMethodsAsync(Order order, List<OrderPaymentMethod> newPayments);
    Task<bool> SaveChangesAsync();
}
