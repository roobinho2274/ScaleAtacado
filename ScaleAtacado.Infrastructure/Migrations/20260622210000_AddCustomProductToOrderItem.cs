using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleAtacado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomProductToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adiciona colunas de nome/código armazenados
            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "OrderItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // 2. Popula com os dados atuais dos produtos
            migrationBuilder.Sql(@"
                UPDATE ""OrderItems"" oi
                SET ""ProductName"" = p.""Name"",
                    ""ProductCode"" = p.""Code""
                FROM ""Products"" p
                WHERE oi.""ProductId"" = p.""Id"";
            ");

            // 3. Torna ProductId nullable (suporte a produtos avulsos)
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "OrderItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(name: "ProductName", table: "OrderItems");
            migrationBuilder.DropColumn(name: "ProductCode", table: "OrderItems");
        }
    }
}
