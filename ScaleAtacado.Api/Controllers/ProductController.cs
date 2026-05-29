using Microsoft.AspNetCore.Mvc;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Services;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController: ControllerBase
{
    private readonly ProductAppService _productAppService;

    public ProductController(ProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            bool result = await _productAppService.CreateProductAsync(createProductDto);
            
            if (result)
            {
                return CreatedAtAction(nameof(CreateProduct), new { id = createProductDto.CompanyId }, createProductDto);
            }
            else
            {
                return BadRequest("Não foi possível criar o produto.");
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno do servidor.", details = ex.Message });
        }
    }

}
