using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("Auditorias");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserId).HasColumnName("UsuarioId");
        builder.Property(a => a.Operation).HasColumnName("Operacao").IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityName).HasColumnName("EntidadeNome").IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).HasColumnName("EntidadeId").HasMaxLength(36);
        builder.Property(a => a.PreviousValue).HasColumnName("ValorAnterior").HasColumnType("text");
        builder.Property(a => a.NewValue).HasColumnName("ValorNovo").HasColumnType("text");
        builder.Property(a => a.UserName).HasColumnName("NomeUsuario").HasMaxLength(200);
        builder.Property(a => a.Description).HasColumnName("Descricao").HasMaxLength(500);
        builder.Property(a => a.Timestamp).HasColumnName("DataHora");
        builder.HasIndex(a => new { a.CompanyId, a.Timestamp })
               .HasDatabaseName("IX_Auditorias_CompanyId_DataHora");
    }
}
