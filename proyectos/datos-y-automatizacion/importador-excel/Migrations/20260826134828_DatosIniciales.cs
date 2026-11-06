using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StockCristales.Migrations
{
    /// <inheritdoc />
    public partial class DatosIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ColoresCristal",
                columns: new[] { "IdColorCristal", "Descripcion" },
                values: new object[,]
                {
                    { 1, "Verde" },
                    { 2, "Verde Plus" },
                    { 3, "Degradé" }
                });

            migrationBuilder.InsertData(
                table: "MarcasCristal",
                columns: new[] { "IdMarcaCristal", "Nombre" },
                values: new object[] { 1, "Pilkington" });

            migrationBuilder.InsertData(
                table: "TiposCristal",
                columns: new[] { "IdTipoCristal", "Descripcion" },
                values: new object[,]
                {
                    { 1, "Parabrisas" },
                    { 2, "Luneta" },
                    { 3, "Puerta delantera izquierda" },
                    { 4, "Puerta delantera derecha" },
                    { 5, "Puerta trasera izquierda" },
                    { 6, "Puerta trasera derecha" },
                    { 7, "Custodia izquierda" },
                    { 8, "Custodia derecha" },
                    { 9, "Ventilete delantero izquierdo" },
                    { 10, "Ventilete delantero derecho" },
                    { 11, "Ventilete trasero izquierdo" },
                    { 12, "Ventilete trasero derecho" },
                    { 13, "Techo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ColoresCristal",
                keyColumn: "IdColorCristal",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ColoresCristal",
                keyColumn: "IdColorCristal",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ColoresCristal",
                keyColumn: "IdColorCristal",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MarcasCristal",
                keyColumn: "IdMarcaCristal",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TiposCristal",
                keyColumn: "IdTipoCristal",
                keyValue: 13);
        }
    }
}
