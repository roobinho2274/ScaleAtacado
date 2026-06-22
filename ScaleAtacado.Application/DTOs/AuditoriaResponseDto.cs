namespace ScaleAtacado.Application.DTOs;

public record AuditLogResponseDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Operation,
    string EntityName,
    string? EntityId,
    string Description,
    string? PreviousValue,
    string? NewValue,
    DateTime Timestamp
);
