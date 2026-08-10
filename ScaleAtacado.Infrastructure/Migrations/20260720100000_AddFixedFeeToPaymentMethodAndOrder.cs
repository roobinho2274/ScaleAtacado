using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260720100000_AddFixedFeeToPaymentMethodAndOrder")]
    public partial class AddFixedFeeToPaymentMethodAndOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""PaymentMethods"" ADD COLUMN IF NOT EXISTS ""FixedFee"" numeric NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE ""Orders"" ADD COLUMN IF NOT EXISTS ""FixedFeeAmount"" numeric NOT NULL DEFAULT 0;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FixedFee", table: "PaymentMethods");
            migrationBuilder.DropColumn(name: "FixedFeeAmount", table: "Orders");
        }
    }
}
