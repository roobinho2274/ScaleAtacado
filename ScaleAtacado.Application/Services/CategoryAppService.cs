using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class CategoryAppService
{
    private readonly ICategoryRepository _repository;

    public CategoryAppService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, Guid companyId)
    {
        var existing = await _repository.GetByNameAsync(dto.Name, companyId);
        if (existing != null)
            return ApiResponse<CategoryResponseDto>.Fail("Já existe uma categoria com este nome.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty
        };

        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();

        return ApiResponse<CategoryResponseDto>.Ok(ToDto(category));
    }

    public async Task<ApiResponse<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, Guid companyId)
    {
        var category = await _repository.GetByIdAsync(id, companyId);
        if (category == null)
            return ApiResponse<CategoryResponseDto>.Fail("Categoria não encontrada.");

        var existing = await _repository.GetByNameAsync(dto.Name, companyId);
        if (existing != null && existing.Id != id)
            return ApiResponse<CategoryResponseDto>.Fail("Já existe outra categoria com este nome.");

        category.Name = dto.Name;
        category.Description = dto.Description ?? string.Empty;

        await _repository.UpdateAsync(category);
        await _repository.SaveChangesAsync();

        return ApiResponse<CategoryResponseDto>.Ok(ToDto(category));
    }

    public async Task<ApiResponse<CategoryResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var category = await _repository.GetByIdAsync(id, companyId);
        if (category == null)
            return ApiResponse<CategoryResponseDto>.Fail("Categoria não encontrada.");

        return ApiResponse<CategoryResponseDto>.Ok(ToDto(category));
    }

    public async Task<ApiResponse<IEnumerable<CategoryResponseDto>>> GetAllAsync(Guid companyId)
    {
        var list = await _repository.GetAllAsync(companyId);
        return ApiResponse<IEnumerable<CategoryResponseDto>>.Ok(list.Select(ToDto));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid companyId)
    {
        var category = await _repository.GetByIdAsync(id, companyId);
        if (category == null)
            return ApiResponse.Fail("Categoria não encontrada.");

        if (await _repository.HasProductsAsync(id))
            return ApiResponse.Fail("Não é possível excluir uma categoria que possui produtos vinculados. Remova ou mova os produtos antes de excluir.");

        await _repository.RemoveAsync(category);
        await _repository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    private static CategoryResponseDto ToDto(Category c) => new(
        c.Id, c.CompanyId, c.Name, c.Description
    );
}
