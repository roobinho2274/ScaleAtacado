using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record UpdatePrintJobStatusDto(
    PrintStatus Status,
    string? ErrorMessage = null
);
