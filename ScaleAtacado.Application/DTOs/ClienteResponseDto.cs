namespace ScaleAtacado.Application.DTOs;

public record ClienteResponseDto(
    Guid Id,
    Guid CompanyId,
    string NomeRazaoSocial,
    string Documento,
    string Endereco,
    string Telefone,
    string? Observacoes,
    bool IsActive,
    DateTime CreatedAt
);
