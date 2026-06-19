using FluentValidation;
using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage("Nome/Razão Social é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("CPF/CNPJ é obrigatório.")
            .Must(IsValidTaxId).WithMessage("CPF ou CNPJ inválido.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Endereço é obrigatório.")
            .MaximumLength(300).WithMessage("Endereço deve ter no máximo 300 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Observações deve ter no máximo 500 caracteres.");
    }

    private static bool IsValidTaxId(string taxId)
    {
        var digits = new string(taxId.Where(char.IsDigit).ToArray());
        return digits.Length == 11 || digits.Length == 14;
    }
}
