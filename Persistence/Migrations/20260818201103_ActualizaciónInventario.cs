using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActualizaciónInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventarioTinta_TintaBase_TintaBaseId",
                table: "InventarioTinta");

            migrationBuilder.DropIndex(
                name: "IX_InventarioTinta_TintaBaseId",
                table: "InventarioTinta");

            migrationBuilder.DropColumn(
                name: "TintaBaseId",
                table: "InventarioTinta");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioTinta_IdTintaBase",
                table: "InventarioTinta",
                column: "IdTintaBase");

            migrationBuilder.AddForeignKey(
                name: "FK_InventarioTinta_TintaBase_IdTintaBase",
                table: "InventarioTinta",
                column: "IdTintaBase",
                principalTable: "TintaBase",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventarioTinta_TintaBase_IdTintaBase",
                table: "InventarioTinta");

            migrationBuilder.DropIndex(
                name: "IX_InventarioTinta_IdTintaBase",
                table: "InventarioTinta");

            migrationBuilder.AddColumn<int>(
                name: "TintaBaseId",
                table: "InventarioTinta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InventarioTinta_TintaBaseId",
                table: "InventarioTinta",
                column: "TintaBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventarioTinta_TintaBase_TintaBaseId",
                table: "InventarioTinta",
                column: "TintaBaseId",
                principalTable: "TintaBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
