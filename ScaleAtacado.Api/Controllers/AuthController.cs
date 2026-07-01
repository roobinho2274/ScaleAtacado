using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Api.Extensions;
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
    private readonly UserAppService _userAppService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtService jwtService,
        AppDbContext context,
        AuditLogAppService auditLog,
        UserAppService userAppService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
        _auditLog = auditLog;
        _userAppService = userAppService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        ApplicationUser? user;

        if (dto.Login.Length == 4 && dto.Login.All(char.IsAsciiDigit))
        {
            var code = int.Parse(dto.Login);
            user = _userManager.Users.FirstOrDefault(u => u.UserCode == code);
        }
        else
        {
            user = await _userManager.FindByEmailAsync(dto.Login);
        }

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(ApiResponse.Fail("E-mail, código ou senha inválidos."));

        if (!user.IsActive)
            return Unauthorized(ApiResponse.Fail("Usuário inativo. Entre em contato com o administrador."));

        var (token, expiresAt) = _jwtService.GenerateToken(user);

        await _auditLog.RecordAsync(
            user.CompanyId, user.Id,
            operation: "Login",
            entityName: "Usuario",
            entityId: user.Id.ToString(),
            userName: user.FullName,
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

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await _auditLog.RecordAsync(
            User.GetCompanyId(), User.GetUserId(),
            operation: "Logoff",
            entityName: "Usuario",
            entityId: User.GetUserId().ToString(),
            userName: User.GetFullName(),
            description: $"{User.GetFullName()} encerrou a sessão"
        );
        return Ok(ApiResponse.Ok());
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Setup([FromBody] SetupDto dto)
    {
        var jaConfigurado = await _context.Companies.AnyAsync();
        if (jaConfigurado)
            return BadRequest(ApiResponse.Fail("O sistema já foi configurado. Use o endpoint de login."));

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

        var adminCode = await _userAppService.GenerateUniqueCodeAsync();

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.AdminName,
            Email = dto.AdminEmail,
            UserName = dto.AdminEmail,
            CompanyId = company.Id,
            Profile = UserProfile.Admin,
            IsActive = true,
            UserCode = adminCode,
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
