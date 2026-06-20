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

            // Margem explícita para garantir que MarginBounds fique dentro da área imprimível.
            // Com Margins(0,0,0,0) o MarginBounds.Right = largura total do papel, que ultrapassa
            // a área imprimível (HardMarginX ≈ 4mm). Usar 20 hundredths (~5mm) é seguro para
            // qualquer impressora térmica.
            doc.DefaultPageSettings.Margins = new Margins(0, 35, 10, 10);

            doc.PrintPage += (sender, e) =>
            {
                var font = new Font("Arial Narrow", 10, FontStyle.Regular, GraphicsUnit.Point);
                var lineHeight = font.GetHeight(e.Graphics!);
                var left = (float)e.MarginBounds.Left;
                var right = (float)e.MarginBounds.Right;
                var bottom = (float)e.MarginBounds.Bottom;
                var y = (float)e.MarginBounds.Top;
                const float pad = 4f;
                var textWidth = right - left - 2 * pad;
                var fmtCenter = new StringFormat { Alignment = StringAlignment.Center };
                var fmtTypo = StringFormat.GenericTypographic;

                while (lineIndex < lines.Length)
                {
                    var line = lines[lineIndex];
                    var midY = y + lineHeight / 2f;

                    if (y + lineHeight > bottom) { e.HasMorePages = true; return; }

                    // --- Separadores: linha GDI de margem a margem ---
                    if (line.Length > 0 && line.All(c => c == '='))
                    {
                        using var pen = new Pen(Color.Black, 1.5f);
                        e.Graphics!.DrawLine(pen, left, midY, right, midY);
                    }
                    else if (line.Length > 0 && line.All(c => c == '-'))
                    {
                        using var pen = new Pen(Color.Black, 0.5f);
                        e.Graphics!.DrawLine(pen, left, midY, right, midY);
                    }

                    // --- Esquerda\tDireita — posicionamento pixel a pixel ---
                    else if (line.Contains('\t'))
                    {
                        var parts = line.Split('\t', 2);
                        var leftText = parts[0];
                        var rightText = parts[1];

                        var rightW = e.Graphics!.MeasureString(rightText, font, new SizeF(9999, 9999), fmtTypo).Width;
                        var rightX = right - pad - rightW;
                        var leftMaxW = rightX - (left + pad) - 4f;

                        leftText = PixelTruncate(leftText, font, e.Graphics, leftMaxW, fmtTypo);

                        e.Graphics.DrawString(leftText, font, Brushes.Black, left + pad, y);
                        e.Graphics.DrawString(rightText, font, Brushes.Black, rightX, y);
                    }

                    // --- Texto centralizado: 3+ espaços iniciais = Center() do formatter ---
                    else if ((line.Length - line.TrimStart().Length) >= 3)
                    {
                        e.Graphics!.DrawString(line.TrimStart(), font, Brushes.Black,
                            new RectangleF(left + pad, y, textWidth, lineHeight), fmtCenter);
                    }

                    // --- Texto comum: trunca por pixel se ultrapassar a largura disponível ---
                    else
                    {
                        var text = PixelTruncate(line, font, e.Graphics!, textWidth, fmtTypo);
                        if (!string.IsNullOrEmpty(text))
                            e.Graphics!.DrawString(text, font, Brushes.Black, left + pad, y);
                    }

                    y += lineHeight;
                    lineIndex++;
                }

                e.HasMorePages = false;
                font.Dispose();
                fmtCenter.Dispose();
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

    // Trunca proporcionalmente para caber em maxPx usando medição GDI real
    private static string PixelTruncate(string text, Font font, Graphics g, float maxPx, StringFormat fmt)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (g.MeasureString(text, font, new SizeF(9999, 9999), fmt).Width <= maxPx) return text;
        while (text.Length > 1)
        {
            text = text[..^1];
            if (g.MeasureString(text + "…", font, new SizeF(9999, 9999), fmt).Width <= maxPx)
                return text + "…";
        }
        return text;
    }
}
