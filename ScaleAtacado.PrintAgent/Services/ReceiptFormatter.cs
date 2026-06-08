using ScaleAtacado.Application.DTOs;

namespace ScaleAtacado.PrintAgent.Services;

public class ReceiptFormatter
{
    private const int Width = 42;

    public string Format(OrderResponseDto order)
    {
        var lines = new List<string>();

        lines.Add(Line('='));
        lines.Add(Center($"PEDIDO DE COMPRA #{order.OrderNumber:D4}"));
        lines.Add(Line('='));
        lines.Add(order.OrderDate.ToLocalTime().ToString("dd/MM/yyyy       HH:mm:ss"));
        lines.Add(string.Empty);
        lines.Add("CLIENTE:");
        lines.Add(Truncate(order.ClienteNome, Width));
        lines.Add(string.Empty);
        lines.Add(Line('='));
        lines.Add(PadBetween("ITEM", "VALOR"));
        lines.Add(Line('-'));

        foreach (var item in order.Items)
        {
            var name = Truncate(item.ProductName, Width - 10);
            lines.Add(name);
            var detail = $"  {item.Quantity} x {item.UnitPrice:N2}";
            var total = item.TotalPrice.ToString("N2");
            lines.Add(PadBetween(detail, total));
        }

        lines.Add(Line('='));
        lines.Add(PadBetween("Subtotal:", $"R$ {order.AmountTotal:N2}"));

        if (order.SurchargePercentage > 0)
            lines.Add(PadBetween($"Acréscimo ({order.SurchargePercentage:N2}%):",
                $"R$ {order.AmountWithSurchargeTotal - order.AmountTotal:N2}"));

        lines.Add(PadBetween("TOTAL:", $"R$ {order.AmountWithSurchargeTotal:N2}"));
        lines.Add(Line('='));
        lines.Add($"Forma: {order.PaymentMethodName}");
        lines.Add(Line('='));
        lines.Add(string.Empty);
        lines.Add(string.Empty);

        return string.Join("\n", lines);
    }

    private static string Line(char ch) => new string(ch, Width);
    private static string Center(string text) => text.PadLeft((Width + text.Length) / 2).PadRight(Width);
    private static string Truncate(string text, int max) => text.Length > max ? text[..max] : text;

    private static string PadBetween(string left, string right)
    {
        var space = Width - left.Length - right.Length;
        return space > 0 ? left + new string(' ', space) + right : left[..(Width - right.Length)] + right;
    }
}
