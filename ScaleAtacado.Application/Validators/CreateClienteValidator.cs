using FluentValidation;
using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.Application.Validators;

public class CreateClienteValidator : AbstractValidator<CreateClienteDto>
{
    public CreateClienteValidator()
    {
        RuleFor(x => x.NomeRazaoSocial)
            .NotEmpty().WithMessage("Nome/Razão Social é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Documento)
            .NotEmpty().WithMessage("CPF/CNPJ é obrigatório.")
            .Must(BeValidDocumento).WithMessage("CPF ou CNPJ inválido.");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("Endereço é obrigatório.")
            .MaximumLength(300).WithMessage("Endereço deve ter no máximo 300 caracteres.");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(500).WithMessage("Observações deve ter no máximo 500 caracteres.");
    }

    private static bool BeValidDocumento(string documento)
    {
        var digits = new string(documento.Where(char.IsDigit).ToArray());
        return digits.Length == 11 || digits.Length == 14;
    }
}
