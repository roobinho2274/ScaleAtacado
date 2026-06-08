namespace ScaleAtacado.Application.DTOs;

public record AuditoriaResponseDto(
    Guid Id,
    Guid UsuarioId,
    string Operacao,
    string EntidadeNome,
    string? EntidadeId,
    string? ValorAnterior,
    string? ValorNovo,
    DateTime DataHora
);
