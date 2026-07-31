using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalSugestao.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResultadoEsperadoToSugestao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResultadoEsperado",
                table: "Sugestoes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultadoEsperado",
                table: "Sugestoes");
        }
    }
}
