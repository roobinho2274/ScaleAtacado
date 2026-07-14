using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogUserAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Auditorias",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomeUsuario",
                table: "Auditorias",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "NomeUsuario",
                table: "Auditorias");
        }
    }
}
