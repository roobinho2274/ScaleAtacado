namespace ScaleAtacado.Domain.Enums;

public enum OrderStatus
{
    Pending      = 0,   // Pendente
    OutForDelivery = 2, // Saiu p/ Entrega
    Delivered    = 3,   // Entregue
    Cancelled    = 4    // Cancelado
}
