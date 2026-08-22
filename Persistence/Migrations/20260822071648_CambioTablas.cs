using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CambioTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleFormula_Formula_IdFormula",
                table: "DetalleFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleFormula_TintaBase_IdTinta",
                table: "DetalleFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_Formula_Empresa_IdEmpresa",
                table: "Formula");

            migrationBuilder.DropForeignKey(
                name: "FK_InventarioTinta_TintaBase_IdTintaBase",
                table: "InventarioTinta");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenFormula_Formula_IdFormula",
                table: "OrdenFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenFormula_OrdenImpresion_IdOrdenImpresion",
                table: "OrdenFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenImpresion_Usuario_IdUsuario",
                table: "OrdenImpresion");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Rol_IdRol",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_Correo",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TintaBase",
                table: "TintaBase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rol",
                table: "Rol");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdenImpresion",
                table: "OrdenImpresion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdenFormula",
                table: "OrdenFormula");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventarioTinta",
                table: "InventarioTinta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Formula",
                table: "Formula");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Empresa",
                table: "Empresa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleFormula",
                table: "DetalleFormula");

            migrationBuilder.RenameTable(
                name: "TintaBase",
                newName: "tintaBase");

            migrationBuilder.RenameTable(
                name: "Rol",
                newName: "rol");

            migrationBuilder.RenameTable(
                name: "OrdenImpresion",
                newName: "ordenImpresion");

            migrationBuilder.RenameTable(
                name: "OrdenFormula",
                newName: "ordenFormula");

            migrationBuilder.RenameTable(
                name: "InventarioTinta",
                newName: "inventarioTinta");

            migrationBuilder.RenameTable(
                name: "Formula",
                newName: "formula");

            migrationBuilder.RenameTable(
                name: "Empresa",
                newName: "empresa");

            migrationBuilder.RenameTable(
                name: "DetalleFormula",
                newName: "detalleFormula");

            migrationBuilder.RenameIndex(
                name: "IX_Rol_Nombre",
                table: "rol",
                newName: "IX_rol_Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenImpresion_IdUsuario",
                table: "ordenImpresion",
                newName: "IX_ordenImpresion_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenFormula_IdOrdenImpresion",
                table: "ordenFormula",
                newName: "IX_ordenFormula_IdOrdenImpresion");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenFormula_IdFormula",
                table: "ordenFormula",
                newName: "IX_ordenFormula_IdFormula");

            migrationBuilder.RenameIndex(
                name: "IX_InventarioTinta_IdTintaBase",
                table: "inventarioTinta",
                newName: "IX_inventarioTinta_IdTintaBase");

            migrationBuilder.RenameIndex(
                name: "IX_Formula_IdEmpresa",
                table: "formula",
                newName: "IX_formula_IdEmpresa");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleFormula_IdTinta",
                table: "detalleFormula",
                newName: "IX_detalleFormula_IdTinta");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleFormula_IdFormula",
                table: "detalleFormula",
                newName: "IX_detalleFormula_IdFormula");

            migrationBuilder.AlterColumn<int>(
                name: "PrimerInicio",
                table: "Usuario",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuario",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuario",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(60)",
                oldMaxLength: 60)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuario",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RolId",
                table: "Usuario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tintaBase",
                table: "tintaBase",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_rol",
                table: "rol",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ordenImpresion",
                table: "ordenImpresion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ordenFormula",
                table: "ordenFormula",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventarioTinta",
                table: "inventarioTinta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_formula",
                table: "formula",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_empresa",
                table: "empresa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_detalleFormula",
                table: "detalleFormula",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_RolId",
                table: "Usuario",
                column: "RolId");

            migrationBuilder.AddForeignKey(
                name: "FK_detalleFormula_formula_IdFormula",
                table: "detalleFormula",
                column: "IdFormula",
                principalTable: "formula",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_detalleFormula_tintaBase_IdTinta",
                table: "detalleFormula",
                column: "IdTinta",
                principalTable: "tintaBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_formula_empresa_IdEmpresa",
                table: "formula",
                column: "IdEmpresa",
                principalTable: "empresa",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_inventarioTinta_tintaBase_IdTintaBase",
                table: "inventarioTinta",
                column: "IdTintaBase",
                principalTable: "tintaBase",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ordenFormula_formula_IdFormula",
                table: "ordenFormula",
                column: "IdFormula",
                principalTable: "formula",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ordenFormula_ordenImpresion_IdOrdenImpresion",
                table: "ordenFormula",
                column: "IdOrdenImpresion",
                principalTable: "ordenImpresion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ordenImpresion_Usuario_IdUsuario",
                table: "ordenImpresion",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_rol_RolId",
                table: "Usuario",
                column: "RolId",
                principalTable: "rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalleFormula_formula_IdFormula",
                table: "detalleFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_detalleFormula_tintaBase_IdTinta",
                table: "detalleFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_formula_empresa_IdEmpresa",
                table: "formula");

            migrationBuilder.DropForeignKey(
                name: "FK_inventarioTinta_tintaBase_IdTintaBase",
                table: "inventarioTinta");

            migrationBuilder.DropForeignKey(
                name: "FK_ordenFormula_formula_IdFormula",
                table: "ordenFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_ordenFormula_ordenImpresion_IdOrdenImpresion",
                table: "ordenFormula");

            migrationBuilder.DropForeignKey(
                name: "FK_ordenImpresion_Usuario_IdUsuario",
                table: "ordenImpresion");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_rol_RolId",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_RolId",
                table: "Usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tintaBase",
                table: "tintaBase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_rol",
                table: "rol");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ordenImpresion",
                table: "ordenImpresion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ordenFormula",
                table: "ordenFormula");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventarioTinta",
                table: "inventarioTinta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_formula",
                table: "formula");

            migrationBuilder.DropPrimaryKey(
                name: "PK_empresa",
                table: "empresa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_detalleFormula",
                table: "detalleFormula");

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "Usuario");

            migrationBuilder.RenameTable(
                name: "tintaBase",
                newName: "TintaBase");

            migrationBuilder.RenameTable(
                name: "rol",
                newName: "Rol");

            migrationBuilder.RenameTable(
                name: "ordenImpresion",
                newName: "OrdenImpresion");

            migrationBuilder.RenameTable(
                name: "ordenFormula",
                newName: "OrdenFormula");

            migrationBuilder.RenameTable(
                name: "inventarioTinta",
                newName: "InventarioTinta");

            migrationBuilder.RenameTable(
                name: "formula",
                newName: "Formula");

            migrationBuilder.RenameTable(
                name: "empresa",
                newName: "Empresa");

            migrationBuilder.RenameTable(
                name: "detalleFormula",
                newName: "DetalleFormula");

            migrationBuilder.RenameIndex(
                name: "IX_rol_Nombre",
                table: "Rol",
                newName: "IX_Rol_Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_ordenImpresion_IdUsuario",
                table: "OrdenImpresion",
                newName: "IX_OrdenImpresion_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_ordenFormula_IdOrdenImpresion",
                table: "OrdenFormula",
                newName: "IX_OrdenFormula_IdOrdenImpresion");

            migrationBuilder.RenameIndex(
                name: "IX_ordenFormula_IdFormula",
                table: "OrdenFormula",
                newName: "IX_OrdenFormula_IdFormula");

            migrationBuilder.RenameIndex(
                name: "IX_inventarioTinta_IdTintaBase",
                table: "InventarioTinta",
                newName: "IX_InventarioTinta_IdTintaBase");

            migrationBuilder.RenameIndex(
                name: "IX_formula_IdEmpresa",
                table: "Formula",
                newName: "IX_Formula_IdEmpresa");

            migrationBuilder.RenameIndex(
                name: "IX_detalleFormula_IdTinta",
                table: "DetalleFormula",
                newName: "IX_DetalleFormula_IdTinta");

            migrationBuilder.RenameIndex(
                name: "IX_detalleFormula_IdFormula",
                table: "DetalleFormula",
                newName: "IX_DetalleFormula_IdFormula");

            migrationBuilder.AlterColumn<int>(
                name: "PrimerInicio",
                table: "Usuario",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuario",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuario",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TintaBase",
                table: "TintaBase",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rol",
                table: "Rol",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdenImpresion",
                table: "OrdenImpresion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdenFormula",
                table: "OrdenFormula",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventarioTinta",
                table: "InventarioTinta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Formula",
                table: "Formula",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Empresa",
                table: "Empresa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleFormula",
                table: "DetalleFormula",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Correo",
                table: "Usuario",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario",
                column: "IdRol");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleFormula_Formula_IdFormula",
                table: "DetalleFormula",
                column: "IdFormula",
                principalTable: "Formula",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleFormula_TintaBase_IdTinta",
                table: "DetalleFormula",
                column: "IdTinta",
                principalTable: "TintaBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Formula_Empresa_IdEmpresa",
                table: "Formula",
                column: "IdEmpresa",
                principalTable: "Empresa",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventarioTinta_TintaBase_IdTintaBase",
                table: "InventarioTinta",
                column: "IdTintaBase",
                principalTable: "TintaBase",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenFormula_Formula_IdFormula",
                table: "OrdenFormula",
                column: "IdFormula",
                principalTable: "Formula",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenFormula_OrdenImpresion_IdOrdenImpresion",
                table: "OrdenFormula",
                column: "IdOrdenImpresion",
                principalTable: "OrdenImpresion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenImpresion_Usuario_IdUsuario",
                table: "OrdenImpresion",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Rol_IdRol",
                table: "Usuario",
                column: "IdRol",
                principalTable: "Rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
