using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ScaleAtacado.Infrastructure.Persistence;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260717120000_AddCopiesToPrintJobs")]
    public partial class AddCopiesToPrintJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""PrintJobs"" ADD COLUMN IF NOT EXISTS ""Copies"" integer NOT NULL DEFAULT 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Copies", table: "PrintJobs");
        }
    }
}
