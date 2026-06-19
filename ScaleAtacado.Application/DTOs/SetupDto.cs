namespace ScaleAtacado.Application.DTOs;

public record SetupDto(
    string  CompanyName,
    string  CompanyCNPJ,
    string  AdminName,
    string  AdminEmail,
    string  AdminPassword,
    string? CompanyAddress = null,
    string? CompanyPhone   = null
);
