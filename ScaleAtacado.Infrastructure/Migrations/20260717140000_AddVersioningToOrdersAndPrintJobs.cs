using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260717140000_AddVersioningToOrdersAndPrintJobs")]
    public partial class AddVersioningToOrdersAndPrintJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Orders"" ADD COLUMN IF NOT EXISTS ""Version"" integer NOT NULL DEFAULT 1;");
            migrationBuilder.Sql(@"ALTER TABLE ""Orders"" ADD COLUMN IF NOT EXISTS ""LastPrintedVersion"" integer;");
            migrationBuilder.Sql(@"ALTER TABLE ""PrintJobs"" ADD COLUMN IF NOT EXISTS ""OrderVersion"" integer NOT NULL DEFAULT 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Version", table: "Orders");
            migrationBuilder.DropColumn(name: "LastPrintedVersion", table: "Orders");
            migrationBuilder.DropColumn(name: "OrderVersion", table: "PrintJobs");
        }
    }
}
