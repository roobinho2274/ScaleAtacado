namespace ScaleAtacado.Shared.Common;

public record AuthTokenDto(
    string Token,
    string FullName,
    string Profile,
    Guid CompanyId,
    DateTime ExpiresAt
);
