namespace ScaleAtacado.Application.DTOs;

public record UpdateOrderPaymentMethodDto(bool IsInstallment, List<OrderPaymentInputDto> Payments);
