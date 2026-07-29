using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalSugestao.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModeracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModeradorId",
                table: "Sugestoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sugestoes_ModeradorId",
                table: "Sugestoes",
                column: "ModeradorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sugestoes_Usuarios_ModeradorId",
                table: "Sugestoes",
                column: "ModeradorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sugestoes_Usuarios_ModeradorId",
                table: "Sugestoes");

            migrationBuilder.DropIndex(
                name: "IX_Sugestoes_ModeradorId",
                table: "Sugestoes");

            migrationBuilder.DropColumn(
                name: "ModeradorId",
                table: "Sugestoes");
        }
    }
}
