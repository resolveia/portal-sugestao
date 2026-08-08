using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalSugestao.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstagioRoadmapToSugestao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstagioRoadmap",
                table: "Sugestoes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstagioRoadmap",
                table: "Sugestoes");
        }
    }
}
