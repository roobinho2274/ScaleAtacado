using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.PrintAgent.Services;

public class ReceiptFormatter
{
    private const int Width = 60;

    public string Format(OrderResponseDto order)
    {
        var lines = new List<string>();
        var surchargeRate = 1 + order.SurchargePercentage / 100m;

        lines.Add(Line('='));
        lines.Add(Center(order.CompanyName ?? "ScaleAtacado"));
        if (!string.IsNullOrWhiteSpace(order.CompanyCNPJ))
            lines.Add(Center($"CNPJ: {order.CompanyCNPJ}"));
        if (!string.IsNullOrWhiteSpace(order.CompanyAddress))
            lines.Add(Center(order.CompanyAddress));
        lines.Add(Center("PEDIDO DE COMPRA"));
        lines.Add(Line('='));
        lines.Add($"Pedido: #{order.OrderNumber:D4}");
        lines.Add($"Data:   {order.OrderDate.ToLocalTime():dd/MM/yyyy HH:mm}");
        lines.Add(Line('-'));
        lines.Add("CLIENTE:");
        lines.Add(Truncate(order.CustomerName, Width));
        lines.Add(Line('-'));
        var paymentNames = string.Join(" + ", order.PaymentMethods.Select(p => p.Name));
        lines.Add($"Pagamento: {Truncate(paymentNames, Width - 11)}");
        lines.Add(Line('='));
        lines.Add(PadBetween("ITEM", "VALOR"));
        lines.Add(Line('-'));

        int num = 1;
        foreach (var item in order.Items)
        {
            var code               = !string.IsNullOrWhiteSpace(item.ProductCode) ? $"[{item.ProductCode}] " : "";
            var nameRaw            = $"#{num} {code}{item.ProductName}";
            var unitWithSurcharge  = Math.Round(item.UnitPrice  * surchargeRate, 2);
            var totalWithSurcharge = Math.Round(item.TotalPrice * surchargeRate, 2);
            var detail             = $"  {item.Quantity} x {unitWithSurcharge:N2}";

            lines.Add(Truncate(nameRaw, Width));
            lines.Add(PadBetween(detail, totalWithSurcharge.ToString("N2")));
            num++;
        }

        lines.Add(Line('='));

        var subtotalDisplay = Math.Round(order.AmountTotal * surchargeRate, 2);
        lines.Add(PadBetween("Subtotal:", $"R$ {subtotalDisplay:N2}"));

        if (order.DiscountAmount > 0)
            lines.Add(PadBetween("Desconto:", $"- R$ {order.DiscountAmount:N2}"));

        lines.Add(PadBetween("TOTAL:", $"R$ {order.AmountWithSurchargeTotal:N2}"));
        lines.Add(Line('='));
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
