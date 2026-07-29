using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260728100000_ChangeOrderItemQuantityToDecimal")]
    public partial class ChangeOrderItemQuantityToDecimal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""OrderItems""
                ALTER COLUMN ""Quantity"" TYPE numeric(18,2)
                USING ""Quantity""::numeric(18,2);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""OrderItems""
                ALTER COLUMN ""Quantity"" TYPE integer
                USING ROUND(""Quantity"")::integer;
            ");
        }
    }
}
