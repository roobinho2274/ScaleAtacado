using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Infrastructure.Identity;
using ScaleAtacado.Infrastructure.Services;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, JwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(ApiResponse.Fail("E-mail ou senha inválidos."));

        if (!user.IsActive)
            return Unauthorized(ApiResponse.Fail("Usuário inativo. Entre em contato com o administrador."));

        var (token, expiresAt) = _jwtService.GenerateToken(user);

        var response = new AuthTokenDto(
            Token: token,
            FullName: user.FullName,
            Profile: user.Profile.ToString(),
            CompanyId: user.CompanyId,
            ExpiresAt: expiresAt
        );

        return Ok(ApiResponse<AuthTokenDto>.Ok(response));
    }
}
