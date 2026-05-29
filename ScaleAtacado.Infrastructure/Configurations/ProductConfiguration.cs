using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class ProductConfiguration: IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(p => p.BaseSalePrice)
               .HasPrecision(18, 2);

        builder.HasIndex(p => new { p.CompanyId, p.Code }).IsUnique();
    }
}
