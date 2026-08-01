using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalSugestao.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexSugestaoStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sugestoes_Status",
                table: "Sugestoes",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sugestoes_Status",
                table: "Sugestoes");
        }
    }
}
