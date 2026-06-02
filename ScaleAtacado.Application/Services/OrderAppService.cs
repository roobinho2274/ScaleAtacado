using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class OrderAppService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IProductRepository _productRepository;

    public OrderAppService(
        IOrderRepository orderRepository,
        IClienteRepository clienteRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _clienteRepository = clienteRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<OrderResponseDto>> CreateAsync(CreateOrderDto dto, Guid companyId, Guid userId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId, companyId);
        if (cliente == null || !cliente.IsActive)
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
        var totalComAcrescimo = subtotal + surcharge;

        var orderNumber = await _orderRepository.GetNextOrderNumberAsync(companyId);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CompanyId = companyId,
            ClienteId = dto.ClienteId,
            PaymentMethodId = dto.PaymentMethodId,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            AmountTotal = subtotal,
            AmountWithSurchargeTotal = totalComAcrescimo,
            DeliveryStatus = DeliveryStatus.AguardandoSeparacao,
            FinancialStatus = FinancialStatus.EmAberto,
            IsLocked = false
        };

        foreach (var item in items)
            item.OrderId = order.Id;

        order.Items = items;

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse<OrderResponseDto>.Ok(
            ToDto(order, cliente.NomeRazaoSocial, paymentMethod.Name, paymentMethod.SurchargePercentage));
    }

    public async Task<ApiResponse<OrderResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse<OrderResponseDto>.Fail("Pedido não encontrado.");

        return ApiResponse<OrderResponseDto>.Ok(
            ToDto(order, order.Cliente.NomeRazaoSocial, order.PaymentMethod.Name, order.PaymentMethod.SurchargePercentage));
    }

    public async Task<ApiResponse<PagedResult<OrderListItemDto>>> GetAllAsync(
        Guid companyId, int page, int pageSize,
        Guid? clienteId, DeliveryStatus? deliveryStatus, FinancialStatus? financialStatus,
        DateTime? from, DateTime? to)
    {
        var (items, totalCount) = await _orderRepository.GetAllAsync(
            companyId, page, pageSize, clienteId, deliveryStatus, financialStatus, from, to);

        var dtos = items.Select(o => new OrderListItemDto(
            o.Id, o.OrderNumber,
            o.Cliente.NomeRazaoSocial,
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

    public async Task<ApiResponse> FinalizarAsync(Guid id, Guid companyId)
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

    public async Task<ApiResponse> DesbloquearAsync(Guid id, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        order.IsLocked = false;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

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

    public async Task<ApiResponse> UpdateFinancialStatusAsync(Guid id, FinancialStatus status, Guid companyId)
    {
        var order = await _orderRepository.GetByIdAsync(id, companyId);
        if (order == null)
            return ApiResponse.Fail("Pedido não encontrado.");

        order.FinancialStatus = status;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    private static OrderResponseDto ToDto(Order o, string clienteNome, string paymentName, decimal surcharge) => new(
        o.Id, o.OrderNumber, o.CompanyId,
        o.ClienteId, clienteNome,
        o.PaymentMethodId, paymentName, surcharge,
        o.UserId, o.OrderDate,
        o.AmountTotal, o.AmountWithSurchargeTotal,
        o.DeliveryStatus, o.FinancialStatus, o.IsLocked,
        o.Items.Select(i => new OrderItemResponseDto(
            i.Id, i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Code,
            i.Quantity, i.UnitPrice, i.TotalPrice
        )).ToList()
    );
}
