using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageQuantityToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackageQuantity",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageQuantity",
                table: "Products");
        }
    }
}
