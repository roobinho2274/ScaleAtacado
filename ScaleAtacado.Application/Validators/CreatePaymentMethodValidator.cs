using FluentValidation;
using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.Application.Validators;

public class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodDto>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.DeadlineDays)
            .GreaterThanOrEqualTo(0).WithMessage("Prazo em dias não pode ser negativo.");

        RuleFor(x => x.SurchargePercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Percentual de acréscimo não pode ser negativo.")
            .LessThanOrEqualTo(100).WithMessage("Percentual de acréscimo não pode exceder 100%.");
    }
}
