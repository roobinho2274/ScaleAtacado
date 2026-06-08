namespace ScaleAtacado.Application.DTOs;

public record PendingPrintJobDto(
    Guid PrintJobId,
    Guid OrderId,
    int TryCount
);
