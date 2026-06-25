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

    public async Task<ApiResponse<OrderResponseDto>> CreateAsync(
        CreateOrderDto dto, Guid companyId, Guid userId, string userName)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, companyId);
        if (customer == null || !customer.IsActive)
            return ApiResponse<OrderResponseDto>.Fail("Cliente não encontrado ou inativo.");

        if (dto.Payments == null || dto.Payments.Count == 0)
            return ApiResponse<OrderResponseDto>.Fail("Selecione ao menos uma forma de pagamento.");

        if (dto.IsInstallment && dto.Payments.Count > 1)
            return ApiResponse<OrderResponseDto>.Fail("Pedidos a prazo permitem apenas uma forma de pagamento.");

        var paymentMethods = new List<PaymentMethod>();
        foreach (var p in dto.Payments)
        {
            var pm = await _paymentMethodRepository.GetByIdAsync(p.PaymentMethodId, companyId);
            if (pm == null || !pm.IsActive)
                return ApiResponse<OrderResponseDto>.Fail("Forma de pagamento não encontrada ou inativa.");
            if (pm.IsInstallment != dto.IsInstallment)
                return ApiResponse<OrderResponseDto>.Fail(
                    $"A forma de pagamento '{pm.Name}' não é compatível com a modalidade selecionada.");
            paymentMethods.Add(pm);
        }

        var items = new List<OrderItem>();
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetProductByIdAsync(itemDto.ProductId, companyId);
            if (product == null || !product.IsActive)
                return ApiResponse<OrderResponseDto>.Fail($"Produto '{itemDto.ProductId}' não encontrado ou inativo.");

            items.Add(new OrderItem
            {
                Id          = Guid.NewGuid(),
                ProductId   = product.Id,
                ProductName = product.Name,
                ProductCode = product.Code,
                Quantity    = itemDto.Quantity,
                UnitPrice   = product.BaseSalePrice,
                TotalPrice  = product.BaseSalePrice * itemDto.Quantity
            });
        }

        var subtotal = items.Sum(i => i.TotalPrice);
        var surchargePercentage = paymentMethods.Count == 1
            ? paymentMethods[0].SurchargePercentage
            : 0m;
        var surcharge = subtotal * (surchargePercentage / 100);
        var discount = Math.Max(0m, dto.DiscountAmount);
        var totalFinal = subtotal + surcharge - discount;

        if (dto.Payments.Count > 1)
        {
            var sumAmounts = dto.Payments.Sum(p => p.Amount);
            if (Math.Abs(sumAmounts - totalFinal) > 0.01m)
                return ApiResponse<OrderResponseDto>.Fail(
                    $"A soma dos valores de pagamento (R$ {sumAmounts:N2}) não corresponde ao total do pedido (R$ {totalFinal:N2}).");
        }

        var orderNumber = await _orderRepository.GetNextOrderNumberAsync(companyId);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CompanyId = companyId,
            CustomerId = dto.CustomerId,
            IsInstallment = dto.IsInstallment,
            SurchargePercentage = surchargePercentage,
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

        var amountsMap = dto.Payments.Count == 1
            ? new Dictionary<Guid, decimal> { { dto.Payments[0].PaymentMethodId, totalFinal } }
            : dto.Payments.ToDictionary(p => p.PaymentMethodId, p => p.Amount);

        order.PaymentMethods = paymentMethods.Select(pm => new OrderPaymentMethod
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PaymentMethodId = pm.Id,
            Amount = amountsMap.GetValueOrDefault(pm.Id, totalFinal),
            PaymentMethod = pm
        }).ToList();

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        var paymentNames = string.Join(" + ", paymentMethods.Select(m => m.Name));
        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "CriarPedido",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            description: $"Pedido #{order.OrderNumber} criado para {customer.LegalName} — {paymentNames} — R$ {totalFinal:N2}"
        );

        return ApiResponse<OrderResponseDto>.Ok(ToDto(order, customer.LegalName, order.PaymentMethods));
    }

    public async Task<ApiResponse<OrderResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse<OrderResponseDto>.Fail("Pedido não encontrado.");

        return ApiResponse<OrderResponseDto>.Ok(
            ToDto(order, order.Customer.LegalName, order.PaymentMethods));
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
            o.IsInstallment,
            string.Join(" + ", o.PaymentMethods.Select(opm => opm.PaymentMethod.Name)),
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

    public async Task<ApiResponse> FinalizeAsync(Guid id, Guid companyId, Guid userId, string userName)
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

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "FinalizarPedido",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            description: $"Pedido #{order.OrderNumber} finalizado"
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse<Guid>> CreatePrintJobAsync(Guid orderId, Guid companyId, Guid userId, string userName)
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

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "ImprimirPedido",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            description: $"Pedido #{order.OrderNumber} enviado para impressão"
        );

        return ApiResponse<Guid>.Ok(printJob.Id);
    }

    public async Task<ApiResponse> UnlockAsync(Guid id, Guid companyId, Guid userId, string userName)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        order.IsLocked = false;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "DesbloquearPedido",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            description: $"Pedido #{order.OrderNumber} desbloqueado para correção"
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateDeliveryStatusAsync(
        Guid id, DeliveryStatus status, Guid companyId, Guid userId, string userName)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        var previousStatus = order.DeliveryStatus;
        order.DeliveryStatus = status;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "AlterarEntrega",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            previousValue: previousStatus.ToString(),
            newValue: status.ToString(),
            description: $"Pedido #{order.OrderNumber}: entrega {DeliveryLabel(previousStatus)} → {DeliveryLabel(status)}"
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateFinancialStatusAsync(
        Guid id, FinancialStatus status, Guid companyId, Guid userId, string userName)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        var previousStatus = order.FinancialStatus;
        order.FinancialStatus = status;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: status == FinancialStatus.Cancelled ? "CancelarPedido" : "AlterarFinanceiro",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            previousValue: previousStatus.ToString(),
            newValue: status.ToString(),
            description: $"Pedido #{order.OrderNumber}: financeiro {FinancialLabel(previousStatus)} → {FinancialLabel(status)}"
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdatePaymentMethodAsync(
        Guid id, UpdateOrderPaymentMethodDto dto, Guid companyId, Guid userId, string userName)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        if (dto.Payments == null || dto.Payments.Count == 0)
            return ApiResponse.Fail("Selecione ao menos uma forma de pagamento.");

        if (dto.IsInstallment && dto.Payments.Count > 1)
            return ApiResponse.Fail("Pedidos a prazo permitem apenas uma forma de pagamento.");

        var newMethods = new List<PaymentMethod>();
        foreach (var p in dto.Payments)
        {
            var pm = await _paymentMethodRepository.GetByIdAsync(p.PaymentMethodId, companyId);
            if (pm == null || !pm.IsActive)
                return ApiResponse.Fail("Forma de pagamento não encontrada ou inativa.");
            if (pm.IsInstallment != dto.IsInstallment)
                return ApiResponse.Fail($"A forma de pagamento '{pm.Name}' não é compatível com a modalidade selecionada.");
            newMethods.Add(pm);
        }

        var surcharge = order.AmountTotal * (
            (newMethods.Count == 1 ? newMethods[0].SurchargePercentage : 0m) / 100m);
        var newTotal = order.AmountTotal + surcharge - order.DiscountAmount;

        if (dto.Payments.Count > 1)
        {
            var sumAmounts = dto.Payments.Sum(p => p.Amount);
            if (Math.Abs(sumAmounts - newTotal) > 0.01m)
                return ApiResponse.Fail(
                    $"A soma dos valores de pagamento (R$ {sumAmounts:N2}) não corresponde ao total do pedido (R$ {newTotal:N2}).");
        }

        var amountsMapUpd = dto.Payments.Count == 1
            ? new Dictionary<Guid, decimal> { { dto.Payments[0].PaymentMethodId, newTotal } }
            : dto.Payments.ToDictionary(p => p.PaymentMethodId, p => p.Amount);

        var previousNames = string.Join(" + ", order.PaymentMethods.Select(opm => opm.PaymentMethod?.Name ?? "?"));
        var newNames = string.Join(" + ", newMethods.Select(m => m.Name));

        order.IsInstallment = dto.IsInstallment;
        order.SurchargePercentage = newMethods.Count == 1 ? newMethods[0].SurchargePercentage : 0m;
        order.PaymentMethods = newMethods.Select(pm => new OrderPaymentMethod
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PaymentMethodId = pm.Id,
            Amount = amountsMapUpd.GetValueOrDefault(pm.Id, newTotal)
        }).ToList();

        order.AmountWithSurchargeTotal = newTotal;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "AlterarPagamento",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            previousValue: previousNames,
            newValue: newNames,
            description: $"Pedido #{order.OrderNumber}: pagamento '{previousNames}' → '{newNames}'"
        );

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateDiscountAsync(
        Guid id, decimal discountAmount, Guid companyId, Guid userId, string userName)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        var previousDiscount = order.DiscountAmount;
        var discount = Math.Max(0m, discountAmount);
        var surcharge = order.AmountTotal * (order.SurchargePercentage / 100);
        order.DiscountAmount = discount;
        order.AmountWithSurchargeTotal = order.AmountTotal + surcharge - discount;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "AplicarDesconto",
            entityName: "Pedido",
            entityId: order.Id.ToString(),
            userName: userName,
            previousValue: $"R$ {previousDiscount:N2}",
            newValue: $"R$ {discount:N2}",
            description: $"Pedido #{order.OrderNumber}: desconto R$ {previousDiscount:N2} → R$ {discount:N2}"
        );

        return ApiResponse.Ok();
    }

    private static string DeliveryLabel(DeliveryStatus s) => s switch
    {
        DeliveryStatus.AwaitingPicking => "Aguardando Separação",
        DeliveryStatus.Picking => "Em Separação",
        DeliveryStatus.OutForDelivery => "Saiu p/ Entrega",
        DeliveryStatus.Delivered => "Entregue",
        DeliveryStatus.Cancelled => "Cancelado",
        _ => s.ToString()
    };

    private static string FinancialLabel(FinancialStatus s) => s switch
    {
        FinancialStatus.Open => "Em Aberto",
        FinancialStatus.Paid => "Pago",
        FinancialStatus.PartiallyPaid => "Parcialmente Pago",
        FinancialStatus.Cancelled => "Cancelado",
        _ => s.ToString()
    };

    private static OrderResponseDto ToDto(Order o, string customerName, IEnumerable<OrderPaymentMethod> opms) => new(
        o.Id, o.OrderNumber, o.CompanyId,
        o.CustomerId, customerName,
        o.IsInstallment,
        o.SurchargePercentage,
        opms.Select(opm => new OrderPaymentMethodDto(opm.PaymentMethodId, opm.PaymentMethod?.Name ?? string.Empty, opm.Amount)).ToList(),
        o.UserId, o.OrderDate,
        o.AmountTotal, o.DiscountAmount, o.AmountWithSurchargeTotal,
        o.DeliveryStatus, o.FinancialStatus, o.IsLocked,
        o.Items.Select(i => new OrderItemResponseDto(
            i.Id, i.ProductId,
            i.ProductName,
            i.ProductCode,
            i.Quantity, i.UnitPrice, i.TotalPrice
        )).ToList()
    );
}
