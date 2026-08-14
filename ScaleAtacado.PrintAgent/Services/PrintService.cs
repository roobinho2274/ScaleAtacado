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

    public bool Print(string receiptText, string? printerName = null, string? logoBase64 = null)
    {
        Image? logoImage = null;
        if (!string.IsNullOrWhiteSpace(logoBase64))
        {
            try
            {
                var raw = logoBase64.Contains(',')
                    ? logoBase64[(logoBase64.IndexOf(',') + 1)..]
                    : logoBase64;
                logoImage = Image.FromStream(new MemoryStream(Convert.FromBase64String(raw)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível decodificar o logo. Imprimindo sem logo.");
            }
        }

        try
        {
            var allLines = receiptText.Split('\n');

            // Extrai linhas de cabeçalho (prefixo "| ") — renderizadas ao lado do logo
            var headerLines = allLines
                .TakeWhile(l => l.StartsWith("| "))
                .Select(l => l[2..])
                .ToArray();

            // Linhas restantes começam após o bloco de cabeçalho
            var lineIndex = headerLines.Length;

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
            // a área imprimível (HardMarginX ≈ 4mm). Usar 35 hundredths (~9mm) é seguro para
            // qualquer impressora térmica.
            doc.DefaultPageSettings.Margins = new Margins(0, 35, 10, 10);

            doc.PrintPage += (sender, e) =>
            {
                using var font     = new Font("Arial Narrow", 10, FontStyle.Regular, GraphicsUnit.Point);
                using var fontBold = new Font("Arial Narrow", 11, FontStyle.Bold,    GraphicsUnit.Point);
                using var fmtCenter = new StringFormat { Alignment = StringAlignment.Center };
                var fmtTypo = StringFormat.GenericTypographic;

                var lineHeight = font.GetHeight(e.Graphics!);
                var left      = (float)e.MarginBounds.Left;
                var right     = (float)e.MarginBounds.Right;
                var bottom    = (float)e.MarginBounds.Bottom;
                var y         = (float)e.MarginBounds.Top;
                const float pad = 4f;
                var textWidth = right - left - 2 * pad;

                // --- Bloco de cabeçalho: logo (esquerda) + dados da empresa (direita) ---
                // Renderizado apenas na primeira página (lineIndex ainda aponta para o início do bloco)
                if (lineIndex == headerLines.Length && (logoImage != null || headerLines.Length > 0))
                {
                    float logoColW = 0f;

                    if (logoImage != null)
                    {
                        // Logo ocupa no máximo 32% da largura e altura equivalente às linhas de cabeçalho
                        var maxLogoW = textWidth * 0.32f;
                        var maxLogoH = lineHeight * Math.Max(headerLines.Length, 3);
                        var scaleW   = maxLogoW / (float)logoImage.Width;
                        var scaleH   = maxLogoH / (float)logoImage.Height;
                        var scale    = Math.Min(1f, Math.Min(scaleW, scaleH));
                        var logoW    = logoImage.Width  * scale;
                        var logoH    = logoImage.Height * scale;
                        logoColW     = logoW + pad * 2;

                        // Centraliza verticalmente em relação ao bloco de texto
                        var blockH = Math.Max(logoH, headerLines.Length * lineHeight);
                        var logoY  = y + (blockH - logoH) / 2f;
                        e.Graphics!.DrawImage(logoImage, left + pad, logoY, logoW, logoH);
                    }

                    // Texto da empresa à direita do logo
                    var textX    = left + pad + logoColW;
                    var textColW = textWidth - logoColW;

                    for (int i = 0; i < headerLines.Length; i++)
                    {
                        var lineFont = i == 0 ? fontBold : font;
                        var text = PixelTruncate(headerLines[i], lineFont, e.Graphics!, textColW, fmtTypo);
                        e.Graphics!.DrawString(text, lineFont, Brushes.Black, textX, y + i * lineHeight);
                    }

                    var blockHeight = Math.Max(
                        logoImage != null ? lineHeight * Math.Max(headerLines.Length, 3) : 0f,
                        headerLines.Length * lineHeight);

                    y += blockHeight + lineHeight * 0.4f;
                }

                // --- Corpo do recibo ---
                while (lineIndex < allLines.Length)
                {
                    var line = allLines[lineIndex];
                    var midY = y + lineHeight / 2f;

                    if (y + lineHeight > bottom) { e.HasMorePages = true; return; }

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
                    else if (line.Length > 0 && line.All(c => c == '*'))
                    {
                        using var pen = new Pen(Color.Black, 0.8f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                        e.Graphics!.DrawLine(pen, left, midY, right, midY);
                    }

                    // --- Esquerda\tDireita — posicionamento pixel a pixel ---
                    else if (line.Contains('\t'))
                    {
                        var parts     = line.Split('\t', 2);
                        var leftText  = parts[0];
                        var rightText = parts[1];

                        var rightW   = e.Graphics!.MeasureString(rightText, font, new SizeF(9999, 9999), fmtTypo).Width;
                        var rightX   = right - pad - rightW;
                        var leftMaxW = rightX - (left + pad) - 4f;

                        leftText = PixelTruncate(leftText, font, e.Graphics, leftMaxW, fmtTypo);

                        e.Graphics.DrawString(leftText,  font, Brushes.Black, left + pad, y);
                        e.Graphics.DrawString(rightText, font, Brushes.Black, rightX,     y);
                    }

                    // --- Texto centralizado: 3+ espaços iniciais = Center() do formatter ---
                    else if ((line.Length - line.TrimStart().Length) >= 3)
                    {
                        e.Graphics!.DrawString(line.TrimStart(), font, Brushes.Black,
                            new RectangleF(left + pad, y, textWidth, lineHeight), fmtCenter);
                    }

                    // --- Texto comum ---
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
        finally
        {
            logoImage?.Dispose();
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
