using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class PrintJobConfiguration : IEntityTypeConfiguration<PrintJobs>
{
    public void Configure(EntityTypeBuilder<PrintJobs> builder)
    {
        builder.ToTable("PrintJobs");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.ErrorMessage).HasMaxLength(500);
    }
}
