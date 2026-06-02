using FluentValidation;
using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.Application.Validators;

public class SetupValidator : AbstractValidator<SetupDto>
{
    public SetupValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Nome da empresa é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.CompanyCNPJ)
            .NotEmpty().WithMessage("CNPJ é obrigatório.")
            .Must(cnpj => new string(cnpj.Where(char.IsDigit).ToArray()).Length == 14)
            .WithMessage("CNPJ inválido.");

        RuleFor(x => x.AdminName)
            .NotEmpty().WithMessage("Nome do administrador é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");
    }
}
