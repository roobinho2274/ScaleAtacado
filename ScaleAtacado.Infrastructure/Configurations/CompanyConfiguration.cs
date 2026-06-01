using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CNPJ).IsRequired().HasMaxLength(18);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.HasIndex(c => c.CNPJ).IsUnique();
    }
}
