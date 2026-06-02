namespace ScaleAtacado.Application.DTOs;

public record UpdateClienteDto(
    string NomeRazaoSocial,
    string Documento,
    string Endereco,
    string Telefone,
    string? Observacoes,
    bool IsActive
);
