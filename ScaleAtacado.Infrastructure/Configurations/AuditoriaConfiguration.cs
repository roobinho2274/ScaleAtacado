using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Operacao).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntidadeNome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntidadeId).HasMaxLength(36);
        builder.Property(a => a.ValorAnterior).HasColumnType("text");
        builder.Property(a => a.ValorNovo).HasColumnType("text");
        builder.HasIndex(a => new { a.CompanyId, a.DataHora });
    }
}
