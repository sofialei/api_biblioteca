using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca_API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndiceUnicoEmailSocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Socios_Email",
                table: "Socios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Socios_Email",
                table: "Socios");
        }
    }
}
