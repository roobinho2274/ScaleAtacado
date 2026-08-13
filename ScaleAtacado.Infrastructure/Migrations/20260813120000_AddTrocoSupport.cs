using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260813120000_AddTrocoSupport")]
    public partial class AddTrocoSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptsChange",
                table: "PaymentMethods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CashReceived",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AcceptsChange", table: "PaymentMethods");
            migrationBuilder.DropColumn(name: "CashReceived", table: "Orders");
        }
    }
}
