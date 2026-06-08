using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class AuditoriaAppService
{
    private readonly IAuditoriaRepository _repository;

    public AuditoriaAppService(IAuditoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task RegistrarAsync(
        Guid companyId,
        Guid usuarioId,
        string operacao,
        string entidadeNome,
        string? entidadeId = null,
        string? valorAnterior = null,
        string? valorNovo = null)
    {
        var auditoria = new Auditoria
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UsuarioId = usuarioId,
            Operacao = operacao,
            EntidadeNome = entidadeNome,
            EntidadeId = entidadeId,
            ValorAnterior = valorAnterior,
            ValorNovo = valorNovo,
            DataHora = DateTime.UtcNow
        };

        await _repository.AddAsync(auditoria);
        await _repository.SaveChangesAsync();
    }

    public async Task<ApiResponse<IEnumerable<AuditoriaResponseDto>>> GetByCompanyAsync(
        Guid companyId, DateTime? from = null, DateTime? to = null)
    {
        var records = await _repository.GetByCompanyAsync(companyId, from, to);

        var dtos = records.Select(a => new AuditoriaResponseDto(
            a.Id, a.UsuarioId, a.Operacao, a.EntidadeNome,
            a.EntidadeId, a.ValorAnterior, a.ValorNovo, a.DataHora
        ));

        return ApiResponse<IEnumerable<AuditoriaResponseDto>>.Ok(dtos);
    }
}
