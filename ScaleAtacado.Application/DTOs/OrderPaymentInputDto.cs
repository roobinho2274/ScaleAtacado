namespace ScaleAtacado.Application.DTOs;

public record OrderPaymentInputDto(Guid PaymentMethodId, decimal Amount);
