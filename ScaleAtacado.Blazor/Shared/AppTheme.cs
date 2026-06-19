using MudBlazor;

namespace ScaleAtacado.Blazor.Shared;

public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary         = "#283593",
            Secondary       = "#00897B",
            AppbarBackground = "#1A237E",
            DrawerBackground = "#F5F7FA",
            DrawerText      = "#37474F",
            DrawerIcon      = "#546E7A",
            Background      = "#EEF0F7",
            Surface         = "#FFFFFF",
            TextPrimary     = "#212121",
            TextSecondary   = "#546E7A",
            Success         = "#2E7D32",
            Warning         = "#E65100",
            Error           = "#C62828",
            Info            = "#0277BD",
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
