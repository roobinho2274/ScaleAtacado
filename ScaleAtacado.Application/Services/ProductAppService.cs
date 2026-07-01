using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class ProductAppService
{
    private readonly IProductRepository _repository;
    private readonly AuditLogAppService _auditLog;

    public ProductAppService(IProductRepository repository, AuditLogAppService auditLog)
    {
        _repository = repository;
        _auditLog = auditLog;
    }

    public async Task<ApiResponse<ProductResponseDto>> CreateAsync(
        CreateProductDto dto, Guid companyId, Guid userId, string userName)
    {
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var existing = await _repository.GetProductByCodeAsync(dto.Code, companyId);
            if (existing != null)
                return ApiResponse<ProductResponseDto>.Fail("Já existe um produto com este código.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = dto.Name,
            Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim(),
            CostPrice = dto.CostPrice,
            ProfitMargin = dto.ProfitMargin,
            BaseSalePrice = CalculateSalePrice(dto.CostPrice, dto.ProfitMargin),
            CategoryId = dto.CategoryId,
            IsActive = true
        };

        await _repository.AddAsync(product);
        await _repository.SaveChangeAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "CriarProduto",
            entityName: "Produto",
            entityId: product.Id.ToString(),
            userName: userName,
            description: $"Produto '{product.Name}' criado — R$ {product.BaseSalePrice:N2}"
        );

        return ApiResponse<ProductResponseDto>.Ok(ToDto(product, string.Empty));
    }

    public async Task<ApiResponse<ProductResponseDto>> UpdateAsync(
        Guid id, UpdateProductDto dto, Guid companyId, Guid userId, string userName)
    {
        var product = await _repository.GetProductByIdAsync(id, companyId);
        if (product == null)
            return ApiResponse<ProductResponseDto>.Fail("Produto não encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != product.Code)
        {
            var existing = await _repository.GetProductByCodeAsync(dto.Code, companyId);
            if (existing != null)
                return ApiResponse<ProductResponseDto>.Fail("Já existe outro produto com este código.");
        }

        var precoAnterior = product.BaseSalePrice;
        var novoPreco = CalculateSalePrice(dto.CostPrice, dto.ProfitMargin);
        var precoAlterado = precoAnterior != novoPreco;

        product.Name = dto.Name;
        product.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim();
        product.CostPrice = dto.CostPrice;
        product.ProfitMargin = dto.ProfitMargin;
        product.BaseSalePrice = novoPreco;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;

        await _repository.UpdateAsync(product);
        await _repository.SaveChangeAsync();

        if (precoAlterado)
        {
            await _auditLog.RecordAsync(
                companyId, userId,
                operation: "AlterarPreco",
                entityName: "Produto",
                entityId: product.Id.ToString(),
                userName: userName,
                previousValue: $"R$ {precoAnterior:N2}",
                newValue: $"R$ {novoPreco:N2}",
                description: $"Produto '{product.Name}': preço R$ {precoAnterior:N2} → R$ {novoPreco:N2}"
            );
        }
        else
        {
            await _auditLog.RecordAsync(
                companyId, userId,
                operation: "EditarProduto",
                entityName: "Produto",
                entityId: product.Id.ToString(),
                userName: userName,
                description: $"Produto '{product.Name}' editado"
            );
        }

        return ApiResponse<ProductResponseDto>.Ok(ToDto(product, product.Category?.Name ?? string.Empty));
    }

    public async Task<ApiResponse<ProductResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var product = await _repository.GetProductByIdAsync(id, companyId);
        if (product == null)
            return ApiResponse<ProductResponseDto>.Fail("Produto não encontrado.");

        return ApiResponse<ProductResponseDto>.Ok(ToDto(product, product.Category?.Name ?? string.Empty));
    }

    public async Task<ApiResponse<PagedResult<ProductResponseDto>>> GetAllAsync(
        Guid companyId, int page, int pageSize, string? search, bool? isActive)
    {
        var (items, totalCount) = await _repository.GetAllAsync(companyId, page, pageSize, search, isActive);
        var dtos = items.Select(p => ToDto(p, p.Category?.Name ?? string.Empty));
        var result = new PagedResult<ProductResponseDto>(dtos, totalCount, page, pageSize);
        return ApiResponse<PagedResult<ProductResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<ProductResponseDto>>> GetAllActiveAsync(Guid companyId)
    {
        var (items, _) = await _repository.GetAllAsync(companyId, 1, 10000, null, true);
        return ApiResponse<IEnumerable<ProductResponseDto>>.Ok(
            items.Select(p => ToDto(p, p.Category?.Name ?? string.Empty)));
    }

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId, Guid userId, string userName)
    {
        var product = await _repository.GetProductByIdAsync(id, companyId);
        if (product == null)
            return ApiResponse.Fail("Produto não encontrado.");

        product.IsActive = false;
        await _repository.UpdateAsync(product);
        await _repository.SaveChangeAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "InativarProduto",
            entityName: "Produto",
            entityId: product.Id.ToString(),
            userName: userName,
            description: $"Produto '{product.Name}' desativado"
        );

        return ApiResponse.Ok();
    }

    private static decimal CalculateSalePrice(decimal costPrice, decimal profitMargin)
        => costPrice * (1 + profitMargin / 100);

    private static ProductResponseDto ToDto(Product p, string categoryName) => new(
        p.Id, p.CompanyId, p.Name, p.Code,
        p.CostPrice, p.ProfitMargin, p.BaseSalePrice,
        p.IsActive, p.CategoryId, categoryName
    );
}
