using MudBlazor;

namespace ScaleAtacado.Blazor.Shared;

public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary          = "#1565C0",
            Secondary        = "#546E7A",
            AppbarBackground = "#0D1B4B",
            DrawerBackground = "#F5F7FA",
            DrawerText       = "#37474F",
            DrawerIcon       = "#1565C0",
            Background       = "#EEF2F8",
            Surface          = "#FFFFFF",
            TextPrimary      = "#212121",
            TextSecondary    = "#546E7A",
            Success          = "#2E7D32",
            Warning          = "#E65100",
            Error            = "#C62828",
            Info             = "#0277BD",
        },
        PaletteDark = new PaletteDark
        {
            Primary          = "#90CAF9",
            Secondary        = "#90A4AE",
            AppbarBackground = "#0D1B4B",
            DrawerBackground = "#1A1F2E",
            DrawerText       = "#CFD8DC",
            DrawerIcon       = "#90CAF9",
            Background       = "#111827",
            Surface          = "#1C2536",
            TextPrimary      = "#E0E0E0",
            TextSecondary    = "#90A4AE",
            Success          = "#66BB6A",
            Warning          = "#FFA726",
            Error            = "#EF5350",
            Info             = "#29B6F6",
            TableLines       = "#2D3748",
            Divider          = "#2D3748",
            ActionDefault    = "#90A4AE",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["DM Sans", "Roboto", "Helvetica", "Arial", "sans-serif"],
            },
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "248px",
        },
    };
}
