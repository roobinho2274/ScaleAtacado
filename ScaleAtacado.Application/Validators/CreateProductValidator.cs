using FluentValidation;
using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Código deve ter no máximo 50 caracteres.");

        RuleFor(x => x.CostPrice)
            .GreaterThan(0).WithMessage("Preço de custo deve ser maior que zero.");

        RuleFor(x => x.ProfitMargin)
            .GreaterThanOrEqualTo(0).WithMessage("Margem de lucro não pode ser negativa.")
            .LessThanOrEqualTo(1000).WithMessage("Margem de lucro não pode exceder 1000%.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Empresa é obrigatória.");
    }
}
