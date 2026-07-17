using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260622210000_AddCustomProductToOrderItem")]
    public partial class AddCustomProductToOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADD COLUMN IF NOT EXISTS evita falha se a coluna já existir de instalação anterior
            migrationBuilder.Sql(@"ALTER TABLE ""OrderItems"" ADD COLUMN IF NOT EXISTS ""ProductName"" character varying(200) NOT NULL DEFAULT '';");
            migrationBuilder.Sql(@"ALTER TABLE ""OrderItems"" ADD COLUMN IF NOT EXISTS ""ProductCode"" character varying(50);");

            // Popula ProductName/Code apenas nas linhas ainda sem nome
            migrationBuilder.Sql(@"
                UPDATE ""OrderItems"" oi
                SET ""ProductName"" = p.""Name"",
                    ""ProductCode"" = p.""Code""
                FROM ""Products"" p
                WHERE oi.""ProductId"" = p.""Id""
                  AND (oi.""ProductName"" = '' OR oi.""ProductName"" IS NULL);
            ");

            // DROP NOT NULL é idempotente no PostgreSQL (não falha se já for nullable)
            migrationBuilder.Sql(@"ALTER TABLE ""OrderItems"" ALTER COLUMN ""ProductId"" DROP NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""OrderItems"" ALTER COLUMN ""ProductId"" SET NOT NULL;");
            migrationBuilder.DropColumn(name: "ProductName", table: "OrderItems");
            migrationBuilder.DropColumn(name: "ProductCode", table: "OrderItems");
        }
    }
}
