using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DesconexionFormula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Formula_OrdenImpresion_OrdenImpresionId",
                table: "Formula");

            migrationBuilder.DropIndex(
                name: "IX_Formula_OrdenImpresionId",
                table: "Formula");

            migrationBuilder.DropColumn(
                name: "OrdenImpresionId",
                table: "Formula");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrdenImpresionId",
                table: "Formula",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Formula_OrdenImpresionId",
                table: "Formula",
                column: "OrdenImpresionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Formula_OrdenImpresion_OrdenImpresionId",
                table: "Formula",
                column: "OrdenImpresionId",
                principalTable: "OrdenImpresion",
                principalColumn: "Id");
        }
    }
}
