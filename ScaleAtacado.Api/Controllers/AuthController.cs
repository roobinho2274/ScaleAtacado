using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Services;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Infrastructure.Identity;
using ScaleAtacado.Infrastructure.Persistence;
using ScaleAtacado.Infrastructure.Services;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly AppDbContext _context;
    private readonly AuditLogAppService _auditLog;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtService jwtService,
        AppDbContext context,
        AuditLogAppService auditLog)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
        _auditLog = auditLog;
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

        await _auditLog.RecordAsync(
            user.CompanyId, user.Id,
            operation: "Login",
            entityName: "Usuario",
            entityId: user.Id.ToString(),
            newValue: $"{user.FullName} — {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC"
        );

        var response = new AuthTokenDto(
            Token: token,
            FullName: user.FullName,
            Profile: user.Profile.ToString(),
            CompanyId: user.CompanyId,
            ExpiresAt: expiresAt
        );

        return Ok(ApiResponse<AuthTokenDto>.Ok(response));
    }

    /// <summary>
    /// Configura o primeiro acesso: cria a empresa e o usuário administrador inicial.
    /// Só funciona quando não existe nenhuma empresa cadastrada no sistema.
    /// </summary>
    [HttpPost("setup")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Setup([FromBody] SetupDto dto)
    {
        var jaConfigurado = await _context.Companies.AnyAsync();
        if (jaConfigurado)
            return BadRequest(ApiResponse.Fail("O sistema já foi configurado. Use o endpoint de login."));

        // Criar empresa
        var company = new Company
        {
            Id      = Guid.NewGuid(),
            Name    = dto.CompanyName,
            CNPJ    = dto.CompanyCNPJ,
            Address = dto.CompanyAddress,
            Phone   = dto.CompanyPhone,
            IsActive = true
        };

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        // Criar usuário admin
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.AdminName,
            Email = dto.AdminEmail,
            UserName = dto.AdminEmail,
            CompanyId = company.Id,
            Profile = UserProfile.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, dto.AdminPassword);
        if (!result.Succeeded)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse.Fail($"Erro ao criar usuário: {errors}"));
        }

        var (token, expiresAt) = _jwtService.GenerateToken(admin);

        var response = new AuthTokenDto(
            Token: token,
            FullName: admin.FullName,
            Profile: admin.Profile.ToString(),
            CompanyId: admin.CompanyId,
            ExpiresAt: expiresAt
        );

        return CreatedAtAction(nameof(Login), ApiResponse<AuthTokenDto>.Ok(response));
    }
}
