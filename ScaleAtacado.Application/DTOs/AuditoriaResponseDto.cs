namespace ScaleAtacado.Application.DTOs;

public record AuditLogResponseDto(
    Guid Id,
    Guid UserId,
    string Operation,
    string EntityName,
    string? EntityId,
    string? PreviousValue,
    string? NewValue,
    DateTime Timestamp
);
