using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByIdAsync(Guid id, Guid companyId);
    Task<PaymentMethod?> GetByNameAsync(string name, Guid companyId);
    Task<IEnumerable<PaymentMethod>> GetAllActiveAsync(Guid companyId);
    Task<IEnumerable<PaymentMethod>> GetAllAsync(Guid companyId);
    Task AddAsync(PaymentMethod paymentMethod);
    Task UpdateAsync(PaymentMethod paymentMethod);
    Task RemoveAsync(PaymentMethod paymentMethod);
    Task<bool> HasOrderPaymentsAsync(Guid paymentMethodId);
    Task<bool> SaveChangesAsync();
}
