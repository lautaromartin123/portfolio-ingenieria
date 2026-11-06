using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StockCristales.Migrations
{
    /// <inheritdoc />
    public partial class RelacionTiposYColores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cristales_TiposCristal_IdTipoCristal",
                table: "Cristales");

            migrationBuilder.DropIndex(
                name: "IX_Cristales_IdTipoCristal",
                table: "Cristales");

            migrationBuilder.DropColumn(
                name: "IdTipoCristal",
                table: "Cristales");

            migrationBuilder.CreateTable(
                name: "RelacionesCristalTipoCristal",
                columns: table => new
                {
                    IdRelacionCristalTipoCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCristal = table.Column<int>(type: "int", nullable: false),
                    IdTipoCristal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionesCristalTipoCristal", x => x.IdRelacionCristalTipoCristal);
                    table.ForeignKey(
                        name: "FK_RelacionesCristalTipoCristal_Cristales_IdCristal",
                        column: x => x.IdCristal,
                        principalTable: "Cristales",
                        principalColumn: "IdCristal",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelacionesCristalTipoCristal_TiposCristal_IdTipoCristal",
                        column: x => x.IdTipoCristal,
                        principalTable: "TiposCristal",
                        principalColumn: "IdTipoCristal",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ColoresCristal",
                columns: new[] { "IdColorCristal", "Descripcion" },
                values: new object[,]
                {
                    { 4, "Azul" },
                    { 5, "Gris" },
                    { 6, "Incoloro" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesCristalTipoCristal_IdCristal_IdTipoCristal",
                table: "RelacionesCristalTipoCristal",
                columns: new[] { "IdCristal", "IdTipoCristal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesCristalTipoCristal_IdTipoCristal",
                table: "RelacionesCristalTipoCristal",
                column: "IdTipoCristal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Esta migración no puede revertirse automáticamente porque " +
                "Cristal-TipoCristal cambió de una relación uno-a-muchos a muchos-a-muchos."
            );
        }






    }
}
