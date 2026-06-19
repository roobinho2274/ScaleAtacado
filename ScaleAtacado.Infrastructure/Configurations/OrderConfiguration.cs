using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.AmountTotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(o => o.AmountWithSurchargeTotal).HasPrecision(18, 2);
        builder.Property(o => o.DeliveryStatus).HasConversion<int>();
        builder.Property(o => o.FinancialStatus).HasConversion<int>();
        builder.Property(o => o.CustomerId).HasColumnName("ClienteId");

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.PaymentMethod)
               .WithMany(p => p.Orders)
               .HasForeignKey(o => o.PaymentMethodId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.CompanyId, o.OrderNumber }).IsUnique();
    }
}
