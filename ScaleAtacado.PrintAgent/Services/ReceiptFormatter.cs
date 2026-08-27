using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.PrintAgent.Services;

public class ReceiptFormatter
{
    private const int Width = 60;

    public string Format(OrderResponseDto order)
    {
        var lines = new List<string>();
        var surchargeRate = 1 + order.SurchargePercentage / 100m;

        // Linhas com prefixo "| " → PrintService renderiza ao lado do logo
        lines.Add($"| {order.CompanyName ?? "ScaleAtacado"}");
        if (!string.IsNullOrWhiteSpace(order.CompanyCNPJ))
            lines.Add($"| CNPJ: {order.CompanyCNPJ}");
        if (!string.IsNullOrWhiteSpace(order.CompanyAddress))
            lines.Add($"| {order.CompanyAddress}");

        lines.Add(Line('-'));

        if (order.Version > 1)
        {
            lines.Add(Line('*'));
            lines.Add(Center($"** DOCUMENTO RETIFICADO - v{order.Version} **"));
            lines.Add(Line('*'));
        }

        lines.Add($"Cliente: {Truncate(order.CustomerName, Width - 9)}");
        if (!string.IsNullOrWhiteSpace(order.CustomerAddress))
            lines.Add(Truncate($"End.: {order.CustomerAddress}", Width));
        if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
            lines.Add(Truncate($"Tel.: {order.CustomerPhone}", Width));

        if (order.PaymentMethods.Count == 1)
        {
            lines.Add($"Pagamento: {Truncate(order.PaymentMethods[0].Name, Width - 11)}");
        }
        else
        {
            lines.Add("Pagamento:");
            foreach (var pm in order.PaymentMethods)
                lines.Add(PadBetween($"  {pm.Name}", $"R$ {pm.Amount:N2}"));
        }

        lines.Add(PadBetween($"Pedido Nº {order.OrderNumber:D4}", order.OrderDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm")));

        lines.Add(Line('='));
        lines.Add(Center("PEDIDO"));
        lines.Add(Line('='));

        foreach (var item in order.Items)
        {
            var code               = !string.IsNullOrWhiteSpace(item.ProductCode) ? $" [{item.ProductCode}]" : "";
            var nameRaw            = $"{item.ProductName}{code}";
            var unitWithSurcharge  = Math.Round(item.UnitPrice  * surchargeRate, 2);
            var totalWithSurcharge = Math.Round(item.TotalPrice * surchargeRate, 2);
            var detail             = $"  {item.Quantity:N2} x R$ {unitWithSurcharge:N2}";

            lines.Add(Truncate(nameRaw, Width));
            lines.Add(PadBetween(detail, $"R$ {totalWithSurcharge:N2}"));
            lines.Add(string.Empty);
        }

        lines.Add(Line('='));

        var subtotalDisplay = Math.Round(order.AmountTotal * surchargeRate, 2);
        lines.Add(PadBetween("Subtotal:", $"R$ {subtotalDisplay:N2}"));

        if (order.DiscountAmount > 0)
            lines.Add(PadBetween("Desconto:", $"- R$ {order.DiscountAmount:N2}"));

        if (order.FixedFeeAmount > 0)
            lines.Add(PadBetween("Taxa Operacional:", $"+ R$ {order.FixedFeeAmount:N2}"));

        lines.Add(PadBetween("TOTAL:", $"R$ {order.AmountWithSurchargeTotal:N2}"));

        if (order.CashReceived > 0)
        {
            var troco = Math.Max(0m, order.CashReceived.Value - order.AmountWithSurchargeTotal);
            lines.Add(PadBetween("Valor Pago:", $"R$ {order.CashReceived.Value:N2}"));
            lines.Add(PadBetween("TROCO:", $"R$ {troco:N2}"));
        }

        lines.Add(Line('='));

        if (!string.IsNullOrWhiteSpace(order.Notes))
        {
            lines.Add("OBS:");
            foreach (var noteLine in order.Notes.Split('\n'))
                lines.Add(Truncate(noteLine.TrimEnd(), Width));
            lines.Add(Line('-'));
        }

        lines.Add(string.Empty);
        lines.Add(Center("Obrigado pela preferencia!"));
        lines.Add(string.Empty);
        lines.Add(string.Empty);

        return string.Join("\n", lines);
    }

    private static string Line(char ch) => new string(ch, Width);
    private static string Center(string text) => text.PadLeft((Width + text.Length) / 2).PadRight(Width);
    private static string Truncate(string text, int max) => text.Length > max ? text[..max] : text;

    // \t como separador — PrintService posiciona cada lado por pixels, não por espaços
    private static string PadBetween(string left, string right) => $"{left}\t{right}";
}
