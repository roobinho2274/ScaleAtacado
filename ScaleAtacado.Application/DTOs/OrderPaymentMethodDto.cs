namespace ScaleAtacado.Application.DTOs;

public record OrderPaymentMethodDto(Guid PaymentMethodId, string Name, decimal Amount);
