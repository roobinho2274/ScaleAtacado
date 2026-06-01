namespace ScaleAtacado.Domain.Entities;

public class Auditoria
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Operacao { get; set; } = string.Empty;
    public string EntidadeNome { get; set; } = string.Empty;
    public string? EntidadeId { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNovo { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
