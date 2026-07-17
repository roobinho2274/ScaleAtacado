using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Domain.Entities;

public class PrintJobs
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public PrintStatus Status { get; set; }
    public int TryCount { get; set; } = 0;
    public int Copies { get; set; } = 1;
    public string? ErrorMessage { get; set; }
    public DateTime OnCreated { get; set; }
    public DateTime? OnProcessed { get; set; }
}
