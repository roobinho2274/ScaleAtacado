namespace ScaleAtacado.Application.DTOs;

public record CreateClienteDto(
    string NomeRazaoSocial,
    string Documento,
    string Endereco,
    string Telefone,
    string? Observacoes
);
