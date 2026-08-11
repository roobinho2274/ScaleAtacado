using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260811100000_RenameDeliveryStatusToOrderStatus")]
    public partial class RenameDeliveryStatusToOrderStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrar registros com Picking (1) para Pending (0) antes de renomear
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""DeliveryStatus"" = 0
                WHERE ""DeliveryStatus"" = 1;
            ");

            migrationBuilder.RenameColumn(
                name: "DeliveryStatus",
                table: "Orders",
                newName: "OrderStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "Orders",
                newName: "DeliveryStatus");
        }
    }
}
