using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreComercial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorreoContacto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRegistro = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TintaBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreTinta = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoHex = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StockActual = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockMinimo_alerta = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TintaBase", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Formula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEmpresa = table.Column<int>(type: "int", nullable: false),
                    NombreColor = table.Column<int>(type: "int", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Formula_Empresa_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    PrimerInicio = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetalleFormula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdFormula = table.Column<int>(type: "int", nullable: false),
                    IdTinta = table.Column<int>(type: "int", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleFormula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleFormula_Formula_IdFormula",
                        column: x => x.IdFormula,
                        principalTable: "Formula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleFormula_TintaBase_IdTinta",
                        column: x => x.IdTinta,
                        principalTable: "TintaBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Logotipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreLogo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlImagen = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEmpresa = table.Column<int>(type: "int", nullable: false),
                    IdFormula = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logotipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logotipo_Empresa_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Logotipo_Formula_IdFormula",
                        column: x => x.IdFormula,
                        principalTable: "Formula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Expires = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Revoked = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LogotipoColor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdFormula = table.Column<int>(type: "int", nullable: false),
                    IdLogotipo = table.Column<int>(type: "int", nullable: false),
                    EsColorprincipal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogotipoColor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogotipoColor_Formula_IdFormula",
                        column: x => x.IdFormula,
                        principalTable: "Formula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogotipoColor_Logotipo_IdLogotipo",
                        column: x => x.IdLogotipo,
                        principalTable: "Logotipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdenImpresion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdLogotipo = table.Column<int>(type: "int", nullable: false),
                    FechaOrden = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    VolumenTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenImpresion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenImpresion_Logotipo_IdLogotipo",
                        column: x => x.IdLogotipo,
                        principalTable: "Logotipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenImpresion_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFormula_IdFormula",
                table: "DetalleFormula",
                column: "IdFormula");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFormula_IdTinta",
                table: "DetalleFormula",
                column: "IdTinta");

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_Nit",
                table: "Empresa",
                column: "Nit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Formula_IdEmpresa",
                table: "Formula",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_Logotipo_IdEmpresa",
                table: "Logotipo",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_Logotipo_IdFormula",
                table: "Logotipo",
                column: "IdFormula");

            migrationBuilder.CreateIndex(
                name: "IX_Logotipo_NombreLogo",
                table: "Logotipo",
                column: "NombreLogo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogotipoColor_IdFormula",
                table: "LogotipoColor",
                column: "IdFormula");

            migrationBuilder.CreateIndex(
                name: "IX_LogotipoColor_IdLogotipo",
                table: "LogotipoColor",
                column: "IdLogotipo");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenImpresion_IdLogotipo",
                table: "OrdenImpresion",
                column: "IdLogotipo");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenImpresion_IdUsuario",
                table: "OrdenImpresion",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UsuarioId",
                table: "RefreshToken",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_Nombre",
                table: "Rol",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Correo",
                table: "Usuario",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario",
                column: "IdRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleFormula");

            migrationBuilder.DropTable(
                name: "LogotipoColor");

            migrationBuilder.DropTable(
                name: "OrdenImpresion");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "TintaBase");

            migrationBuilder.DropTable(
                name: "Logotipo");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Formula");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Empresa");
        }
    }
}
