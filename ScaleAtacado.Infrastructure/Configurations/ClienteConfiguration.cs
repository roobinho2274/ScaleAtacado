using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.NomeRazaoSocial).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Documento).IsRequired().HasMaxLength(18);
        builder.Property(c => c.Endereco).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Observacoes).HasMaxLength(500);
        builder.HasIndex(c => new { c.CompanyId, c.Documento }).IsUnique();
    }
}
