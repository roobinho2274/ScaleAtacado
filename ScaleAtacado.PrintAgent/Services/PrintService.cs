using System.Drawing;
using System.Drawing.Printing;

namespace ScaleAtacado.PrintAgent.Services;

public class PrintService
{
    private readonly ILogger<PrintService> _logger;

    public PrintService(ILogger<PrintService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> GetInstalledPrinters()
        => PrinterSettings.InstalledPrinters.Cast<string>();

    public bool Print(string receiptText, string? printerName = null)
    {
        try
        {
            var lines = receiptText.Split('\n');
            var lineIndex = 0;

            var doc = new PrintDocument();

            if (!string.IsNullOrWhiteSpace(printerName))
                doc.PrinterSettings.PrinterName = printerName;

            if (!doc.PrinterSettings.IsValid)
            {
                _logger.LogWarning("Impressora '{Printer}' não encontrada. Usando impressora padrão.", printerName);
                doc.PrinterSettings.PrinterName = string.Empty;
            }

            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            doc.PrintPage += (sender, e) =>
            {
                var font = new Font("Arial Narrow", 9, FontStyle.Regular, GraphicsUnit.Point);
                var lineHeight = font.GetHeight(e.Graphics!);
                var y = (float)e.MarginBounds.Top;

                while (lineIndex < lines.Length)
                {
                    if (y + lineHeight > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    e.Graphics!.DrawString(lines[lineIndex], font, Brushes.Black, e.MarginBounds.Left, y);
                    y += lineHeight;
                    lineIndex++;
                }

                e.HasMorePages = false;
                font.Dispose();
            };

            doc.Print();
            _logger.LogInformation("Cupom impresso na impressora '{Printer}'.", doc.PrinterSettings.PrinterName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao imprimir cupom.");
            return false;
        }
    }
}
