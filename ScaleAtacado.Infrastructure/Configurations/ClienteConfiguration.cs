using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.LegalName).HasColumnName("NomeRazaoSocial").IsRequired().HasMaxLength(200);
        builder.Property(c => c.TaxId).HasColumnName("Documento").IsRequired().HasMaxLength(18);
        builder.Property(c => c.Address).HasColumnName("Endereco").IsRequired().HasMaxLength(300);
        builder.Property(c => c.Phone).HasColumnName("Telefone").IsRequired().HasMaxLength(20);
        builder.Property(c => c.Notes).HasColumnName("Observacoes").HasMaxLength(500);
        builder.HasIndex(c => new { c.CompanyId, c.TaxId })
               .HasDatabaseName("IX_Clientes_CompanyId_Documento")
               .IsUnique();
    }
}
