using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Infrastructure.Configurations;

public class OrderPaymentMethodConfiguration : IEntityTypeConfiguration<OrderPaymentMethod>
{
    public void Configure(EntityTypeBuilder<OrderPaymentMethod> builder)
    {
        builder.ToTable("OrderPaymentMethods");
        builder.HasKey(opm => opm.Id);

        builder.Property(opm => opm.Amount)
               .HasColumnType("numeric(18,2)")
               .HasDefaultValue(0m);

        builder.HasOne(opm => opm.Order)
               .WithMany(o => o.PaymentMethods)
               .HasForeignKey(opm => opm.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(opm => opm.PaymentMethod)
               .WithMany(pm => pm.OrderPaymentMethods)
               .HasForeignKey(opm => opm.PaymentMethodId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
