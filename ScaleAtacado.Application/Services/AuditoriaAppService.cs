using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class AuditLogAppService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogAppService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task RecordAsync(
        Guid companyId,
        Guid userId,
        string operation,
        string entityName,
        string? entityId = null,
        string? previousValue = null,
        string? newValue = null,
        string userName = "",
        string description = "")
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = userId,
            UserName = userName,
            Operation = operation,
            EntityName = entityName,
            EntityId = entityId,
            Description = description,
            PreviousValue = previousValue,
            NewValue = newValue,
            Timestamp = DateTime.UtcNow
        };

        await _repository.AddAsync(auditLog);
        await _repository.SaveChangesAsync();
    }

    public async Task<ApiResponse<IEnumerable<AuditLogResponseDto>>> GetByCompanyAsync(
        Guid companyId, DateTime? from = null, DateTime? to = null)
    {
        var records = await _repository.GetByCompanyAsync(companyId, from, to);

        var dtos = records.Select(a => new AuditLogResponseDto(
            a.Id, a.UserId, a.UserName, a.Operation, a.EntityName,
            a.EntityId, a.Description, a.PreviousValue, a.NewValue, a.Timestamp
        ));

        return ApiResponse<IEnumerable<AuditLogResponseDto>>.Ok(dtos);
    }
}
