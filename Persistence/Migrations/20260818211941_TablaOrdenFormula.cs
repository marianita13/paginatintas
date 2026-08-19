using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TablaOrdenFormula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenFormula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdFormula = table.Column<int>(type: "int", nullable: false),
                    IdOrdenImpresion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenFormula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenFormula_Formula_IdFormula",
                        column: x => x.IdFormula,
                        principalTable: "Formula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenFormula_OrdenImpresion_IdOrdenImpresion",
                        column: x => x.IdOrdenImpresion,
                        principalTable: "OrdenImpresion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenFormula_IdFormula",
                table: "OrdenFormula",
                column: "IdFormula");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenFormula_IdOrdenImpresion",
                table: "OrdenFormula",
                column: "IdOrdenImpresion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenFormula");
        }
    }
}
