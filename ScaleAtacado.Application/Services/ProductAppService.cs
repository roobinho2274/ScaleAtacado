using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Services;

/// <summary>
/// Serviço de aplicação para operações de produto: valida unicidade de código por empresa, calcula o preço de venda
/// base a partir do custo e margem e persiste o produto via IProductRepository.
/// </summary>
/// <remarks>Injeta IProductRepository via construtor. O método CreateProductAsync lança InvalidOperationException
/// quando já existe um produto com o mesmo código para a empresa. O preço de venda base é calculado como CostPrice * (1
/// + ProfitMargin / 100). Retorna true se as alterações forem salvas com sucesso.</remarks>
public class ProductAppService
{
    private readonly IProductRepository _productRepository;

    public ProductAppService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<bool> CreateProductAsync(CreateProductDto createProductDto)
    {
        var existingProduct = await _productRepository.GetProductByCodeAsync(createProductDto.Code, createProductDto.CompanyId);
        if (existingProduct != null)
        {
            throw new InvalidOperationException("Já existe um produto com o mesmo código para esta empresa.");
        }

        decimal marginMultiplier = 1 + (createProductDto.ProfitMargin / 100);
        decimal calculatedSalePrice = createProductDto.CostPrice * marginMultiplier;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = createProductDto.CompanyId,
            Name = createProductDto.Name,
            Code = createProductDto.Code,
            CostPrice = createProductDto.CostPrice,
            ProfitMargin = createProductDto.ProfitMargin,
            BaseSalePrice = calculatedSalePrice,
            CategoryId = createProductDto.CategoryId
        };
        await _productRepository.AddAsync(product);
        return await _productRepository.SaveChangeAsync();
    }
}
