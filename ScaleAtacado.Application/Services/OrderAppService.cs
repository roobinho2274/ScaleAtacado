using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class OrderAppService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPrintJobRepository _printJobRepository;
    private readonly AuditLogAppService _auditLog;

    public OrderAppService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IProductRepository productRepository,
        IPrintJobRepository printJobRepository,
        AuditLogAppService auditLog)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _productRepository = productRepository;
        _printJobRepository = printJobRepository;
        _auditLog = auditLog;
    }

    public async Task<ApiResponse<OrderResponseDto>> CreateAsync(CreateOrderDto dto, Guid companyId, Guid userId)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, companyId);
        if (customer == null || !customer.IsActive)
            return ApiResponse<OrderResponseDto>.Fail("Cliente não encontrado ou inativo.");

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId, companyId);
        if (paymentMethod == null || !paymentMethod.IsActive)
            return ApiResponse<OrderResponseDto>.Fail("Forma de pagamento não encontrada ou inativa.");

        var items = new List<OrderItem>();
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetProductByIdAsync(itemDto.ProductId, companyId);
            if (product == null || !product.IsActive)
                return ApiResponse<OrderResponseDto>.Fail($"Produto '{itemDto.ProductId}' não encontrado ou inativo.");

            items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.BaseSalePrice,
                TotalPrice = product.BaseSalePrice * itemDto.Quantity
            });
        }

        var subtotal = items.Sum(i => i.TotalPrice);
        var surcharge = subtotal * (paymentMethod.SurchargePercentage / 100);
        var discount = Math.Max(0m, dto.DiscountAmount);
        var totalFinal = subtotal + surcharge - discount;

        var orderNumber = await _orderRepository.GetNextOrderNumberAsync(companyId);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CompanyId = companyId,
            CustomerId = dto.CustomerId,
            PaymentMethodId = dto.PaymentMethodId,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            AmountTotal = subtotal,
            DiscountAmount = discount,
            AmountWithSurchargeTotal = totalFinal,
            DeliveryStatus = DeliveryStatus.AwaitingPicking,
            FinancialStatus = FinancialStatus.Open,
            IsLocked = false
        };

        foreach (var item in items)
            item.OrderId = order.Id;

        order.Items = items;

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse<OrderResponseDto>.Ok(
            ToDto(order, customer.LegalName, paymentMethod.Name, paymentMethod.SurchargePercentage));
    }

    public async Task<ApiResponse<OrderResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse<OrderResponseDto>.Fail("Pedido não encontrado.");

        return ApiResponse<OrderResponseDto>.Ok(
            ToDto(order, order.Customer.LegalName, order.PaymentMethod.Name, order.PaymentMethod.SurchargePercentage));
    }

    public async Task<ApiResponse<PagedResult<OrderListItemDto>>> GetAllAsync(
        Guid companyId, int page, int pageSize,
        Guid? customerId, DeliveryStatus? deliveryStatus, FinancialStatus? financialStatus,
        DateTime? from, DateTime? to)
    {
        var (items, totalCount) = await _orderRepository.GetAllAsync(
            companyId, page, pageSize, customerId, deliveryStatus, financialStatus, from, to);

        var dtos = items.Select(o => new OrderListItemDto(
            o.Id, o.OrderNumber,
            o.Customer.LegalName,
            o.PaymentMethod.Name,
            o.OrderDate,
            o.AmountTotal,
            o.AmountWithSurchargeTotal,
            o.DeliveryStatus,
            o.FinancialStatus,
            o.IsLocked
        ));

        return ApiResponse<PagedResult<OrderListItemDto>>.Ok(
            new PagedResult<OrderListItemDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<ApiResponse> FinalizeAsync(Guid id, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        if (order.IsLocked)
            return ApiResponse.Fail("Pedido já está finalizado.");

        if (!order.Items.Any())
            return ApiResponse.Fail("Não é possível finalizar um pedido sem itens.");

        order.IsLocked = true;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse<Guid>> CreatePrintJobAsync(Guid orderId, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, companyId);
        if (order == null)
            return ApiResponse<Guid>.Fail("Pedido não encontrado.");

        var printJob = new Domain.Entities.PrintJobs
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = Domain.Enums.PrintStatus.OnHoldem,
            TryCount = 0,
            OnCreated = DateTime.UtcNow
        };

        await _printJobRepository.AddAsync(printJob);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse<Guid>.Ok(printJob.Id);
    }

    public async Task<ApiResponse> UnlockAsync(Guid id, Guid companyId, Guid userId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        order.IsLocked = false;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "UnlockOrder",
            entityName: "Order",
            entityId: order.Id.ToString(),
            previousValue: $"Pedido #{order.OrderNumber} bloqueado",
            newValue: $"Pedido #{order.OrderNumber} desbloqueado para correção"
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateDeliveryStatusAsync(Guid id, DeliveryStatus status, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        order.DeliveryStatus = status;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateFinancialStatusAsync(Guid id, FinancialStatus status, Guid companyId, Guid userId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        var previousStatus = order.FinancialStatus;
        order.FinancialStatus = status;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        if (status == FinancialStatus.Cancelled)
        {
            await _auditLog.RecordAsync(
                companyId, userId,
                operation: "CancelOrder",
                entityName: "Order",
                entityId: order.Id.ToString(),
                previousValue: previousStatus.ToString(),
                newValue: status.ToString()
            );
        }

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdatePaymentMethodAsync(Guid id, Guid paymentMethodId, Guid companyId, Guid userId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        var newPaymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId, companyId);
        if (newPaymentMethod == null || !newPaymentMethod.IsActive)
            return ApiResponse.Fail("Forma de pagamento não encontrada ou inativa.");

        var previousName = order.PaymentMethod?.Name ?? order.PaymentMethodId.ToString();

        order.PaymentMethodId = paymentMethodId;
        var surcharge = order.AmountTotal * (newPaymentMethod.SurchargePercentage / 100m);
        order.AmountWithSurchargeTotal = order.AmountTotal + surcharge - order.DiscountAmount;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "ChangePaymentMethod",
            entityName: "Order",
            entityId: order.Id.ToString(),
            previousValue: previousName,
            newValue: newPaymentMethod.Name
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateDiscountAsync(Guid id, decimal discountAmount, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(order.PaymentMethodId, companyId);
        var surchargePercentage = paymentMethod?.SurchargePercentage ?? 0m;

        var discount = Math.Max(0m, discountAmount);
        var surcharge = order.AmountTotal * (surchargePercentage / 100);
        order.DiscountAmount = discount;
        order.AmountWithSurchargeTotal = order.AmountTotal + surcharge - discount;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    private static OrderResponseDto ToDto(Order o, string customerName, string paymentName, decimal surcharge) => new(
        o.Id, o.OrderNumber, o.CompanyId,
        o.CustomerId, customerName,
        o.PaymentMethodId, paymentName, surcharge,
        o.UserId, o.OrderDate,
        o.AmountTotal, o.DiscountAmount, o.AmountWithSurchargeTotal,
        o.DeliveryStatus, o.FinancialStatus, o.IsLocked,
        o.Items.Select(i => new OrderItemResponseDto(
            i.Id, i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Code,
            i.Quantity, i.UnitPrice, i.TotalPrice
        )).ToList()
    );
}
