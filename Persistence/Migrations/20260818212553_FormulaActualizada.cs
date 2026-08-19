using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormulaActualizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Formula_OrdenImpresion_IdOrdenImpresion",
                table: "Formula");

            migrationBuilder.RenameColumn(
                name: "IdOrdenImpresion",
                table: "Formula",
                newName: "OrdenImpresionId");

            migrationBuilder.RenameIndex(
                name: "IX_Formula_IdOrdenImpresion",
                table: "Formula",
                newName: "IX_Formula_OrdenImpresionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Formula_OrdenImpresion_OrdenImpresionId",
                table: "Formula",
                column: "OrdenImpresionId",
                principalTable: "OrdenImpresion",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Formula_OrdenImpresion_OrdenImpresionId",
                table: "Formula");

            migrationBuilder.RenameColumn(
                name: "OrdenImpresionId",
                table: "Formula",
                newName: "IdOrdenImpresion");

            migrationBuilder.RenameIndex(
                name: "IX_Formula_OrdenImpresionId",
                table: "Formula",
                newName: "IX_Formula_IdOrdenImpresion");

            migrationBuilder.AddForeignKey(
                name: "FK_Formula_OrdenImpresion_IdOrdenImpresion",
                table: "Formula",
                column: "IdOrdenImpresion",
                principalTable: "OrdenImpresion",
                principalColumn: "Id");
        }
    }
}
