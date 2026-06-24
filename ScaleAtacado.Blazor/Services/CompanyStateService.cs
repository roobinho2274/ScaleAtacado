namespace ScaleAtacado.Blazor.Services;

public class CompanyStateService
{
    public string? LogoBase64 { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;

    public event Action? OnChange;

    public void Set(string companyName, string? logoBase64)
    {
        CompanyName = companyName;
        LogoBase64  = logoBase64;
        OnChange?.Invoke();
    }
}
