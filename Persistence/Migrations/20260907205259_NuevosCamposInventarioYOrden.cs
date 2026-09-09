using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevosCamposInventarioYOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedidaLamina",
                table: "ordenimpresion",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MontajeImpresion",
                table: "ordenimpresion",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaOrden",
                table: "inventariotinta",
                type: "datetime",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Formula",
                table: "inventariotinta",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ordenimpresion_NumeroOrden",
                table: "ordenimpresion",
                column: "NumeroOrden",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ordenimpresion_NumeroOrden",
                table: "ordenimpresion");

            migrationBuilder.DropColumn(
                name: "MedidaLamina",
                table: "ordenimpresion");

            migrationBuilder.DropColumn(
                name: "MontajeImpresion",
                table: "ordenimpresion");

            migrationBuilder.DropColumn(
                name: "FechaOrden",
                table: "inventariotinta");

            migrationBuilder.DropColumn(
                name: "Formula",
                table: "inventariotinta");
        }
    }
}
