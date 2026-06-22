using FluentValidation;
using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Cliente é obrigatório.");

        RuleFor(x => x.Payments)
            .NotEmpty().WithMessage("Selecione ao menos uma forma de pagamento.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve ter pelo menos um item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Produto é obrigatório.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
        });
    }
}
