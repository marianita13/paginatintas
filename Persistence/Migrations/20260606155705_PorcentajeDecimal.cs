using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PorcentajeDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Porcentaje",
                table: "DetalleFormula",
                type: "decimal(10,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Porcentaje",
                table: "DetalleFormula",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)");
        }
    }
}
