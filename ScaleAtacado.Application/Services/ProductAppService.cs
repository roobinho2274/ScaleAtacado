using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class ProductAppService
{
    private readonly IProductRepository _repository;

    public ProductAppService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<ProductResponseDto>> CreateAsync(CreateProductDto dto, Guid companyId)
    {
        var existing = await _repository.GetProductByCodeAsync(dto.Code!, companyId);
        if (existing != null)
            return ApiResponse<ProductResponseDto>.Fail("Já existe um produto com este código.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = dto.Name,
            Code = dto.Code,
            CostPrice = dto.CostPrice,
            ProfitMargin = dto.ProfitMargin,
            BaseSalePrice = CalculateSalePrice(dto.CostPrice, dto.ProfitMargin),
            CategoryId = dto.CategoryId,
            IsActive = true
        };

        await _repository.AddAsync(product);
        await _repository.SaveChangeAsync();

        return ApiResponse<ProductResponseDto>.Ok(ToDto(product, string.Empty));
    }

    public async Task<ApiResponse<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, Guid companyId)
    {
        var product = await _repository.GetProductByIdAsync(id, companyId);
        if (product == null)
            return ApiResponse<ProductResponseDto>.Fail("Produto não encontrado.");

        if (dto.Code != product.Code)
        {
            var existing = await _repository.GetProductByCodeAsync(dto.Code!, companyId);
            if (existing != null)
                return ApiResponse<ProductResponseDto>.Fail("Já existe outro produto com este código.");
        }

        product.Name = dto.Name;
        product.Code = dto.Code;
        product.CostPrice = dto.CostPrice;
        product.ProfitMargin = dto.ProfitMargin;
        product.BaseSalePrice = CalculateSalePrice(dto.CostPrice, dto.ProfitMargin);
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;

        await _repository.UpdateAsync(product);
        await _repository.SaveChangeAsync();

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

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId)
    {
        var product = await _repository.GetProductByIdAsync(id, companyId);
        if (product == null)
            return ApiResponse.Fail("Produto não encontrado.");

        product.IsActive = false;
        await _repository.UpdateAsync(product);
        await _repository.SaveChangeAsync();

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
