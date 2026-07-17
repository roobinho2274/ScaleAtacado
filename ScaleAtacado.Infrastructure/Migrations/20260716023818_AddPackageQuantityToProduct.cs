using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageQuantityToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADD COLUMN IF NOT EXISTS evita falha se a coluna já existir de instalação anterior
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" ADD COLUMN IF NOT EXISTS ""PackageQuantity"" integer NOT NULL DEFAULT 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageQuantity",
                table: "Products");
        }
    }
}
