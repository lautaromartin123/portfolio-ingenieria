using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockCristales.Migrations
{
    /// <inheritdoc />
    public partial class CreacionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaracteristicasCristal",
                columns: table => new
                {
                    IdCaracteristicaCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaracteristicasCristal", x => x.IdCaracteristicaCristal);
                });

            migrationBuilder.CreateTable(
                name: "ColoresCristal",
                columns: table => new
                {
                    IdColorCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColoresCristal", x => x.IdColorCristal);
                });

            migrationBuilder.CreateTable(
                name: "MarcasCristal",
                columns: table => new
                {
                    IdMarcaCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasCristal", x => x.IdMarcaCristal);
                });

            migrationBuilder.CreateTable(
                name: "MarcasVehiculo",
                columns: table => new
                {
                    IdMarcaVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasVehiculo", x => x.IdMarcaVehiculo);
                });

            migrationBuilder.CreateTable(
                name: "OrigenesCristal",
                columns: table => new
                {
                    IdOrigenCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrigenesCristal", x => x.IdOrigenCristal);
                });

            migrationBuilder.CreateTable(
                name: "TiposCristal",
                columns: table => new
                {
                    IdTipoCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCristal", x => x.IdTipoCristal);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    IdVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Modelo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdMarcaVehiculo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.IdVehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculos_MarcasVehiculo_IdMarcaVehiculo",
                        column: x => x.IdMarcaVehiculo,
                        principalTable: "MarcasVehiculo",
                        principalColumn: "IdMarcaVehiculo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cristales",
                columns: table => new
                {
                    IdCristal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdTipoCristal = table.Column<int>(type: "int", nullable: false),
                    IdMarcaCristal = table.Column<int>(type: "int", nullable: false),
                    IdColorCristal = table.Column<int>(type: "int", nullable: true),
                    IdOrigenCristal = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cristales", x => x.IdCristal);
                    table.ForeignKey(
                        name: "FK_Cristales_ColoresCristal_IdColorCristal",
                        column: x => x.IdColorCristal,
                        principalTable: "ColoresCristal",
                        principalColumn: "IdColorCristal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cristales_MarcasCristal_IdMarcaCristal",
                        column: x => x.IdMarcaCristal,
                        principalTable: "MarcasCristal",
                        principalColumn: "IdMarcaCristal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cristales_OrigenesCristal_IdOrigenCristal",
                        column: x => x.IdOrigenCristal,
                        principalTable: "OrigenesCristal",
                        principalColumn: "IdOrigenCristal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cristales_TiposCristal_IdTipoCristal",
                        column: x => x.IdTipoCristal,
                        principalTable: "TiposCristal",
                        principalColumn: "IdTipoCristal",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelacionesCristalCaracteristica",
                columns: table => new
                {
                    IdRelacionCristalCaracteristica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCristal = table.Column<int>(type: "int", nullable: false),
                    IdCaracteristicaCristal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionesCristalCaracteristica", x => x.IdRelacionCristalCaracteristica);
                    table.ForeignKey(
                        name: "FK_RelacionesCristalCaracteristica_CaracteristicasCristal_IdCaracteristicaCristal",
                        column: x => x.IdCaracteristicaCristal,
                        principalTable: "CaracteristicasCristal",
                        principalColumn: "IdCaracteristicaCristal",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelacionesCristalCaracteristica_Cristales_IdCristal",
                        column: x => x.IdCristal,
                        principalTable: "Cristales",
                        principalColumn: "IdCristal",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelacionesCristalVehiculo",
                columns: table => new
                {
                    IdRelacionCristalVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCristal = table.Column<int>(type: "int", nullable: false),
                    IdVehiculo = table.Column<int>(type: "int", nullable: false),
                    AnioDesde = table.Column<int>(type: "int", nullable: false),
                    AnioHasta = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionesCristalVehiculo", x => x.IdRelacionCristalVehiculo);
                    table.ForeignKey(
                        name: "FK_RelacionesCristalVehiculo_Cristales_IdCristal",
                        column: x => x.IdCristal,
                        principalTable: "Cristales",
                        principalColumn: "IdCristal",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelacionesCristalVehiculo_Vehiculos_IdVehiculo",
                        column: x => x.IdVehiculo,
                        principalTable: "Vehiculos",
                        principalColumn: "IdVehiculo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cristales_Codigo",
                table: "Cristales",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cristales_IdColorCristal",
                table: "Cristales",
                column: "IdColorCristal");

            migrationBuilder.CreateIndex(
                name: "IX_Cristales_IdMarcaCristal",
                table: "Cristales",
                column: "IdMarcaCristal");

            migrationBuilder.CreateIndex(
                name: "IX_Cristales_IdOrigenCristal",
                table: "Cristales",
                column: "IdOrigenCristal");

            migrationBuilder.CreateIndex(
                name: "IX_Cristales_IdTipoCristal",
                table: "Cristales",
                column: "IdTipoCristal");

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesCristalCaracteristica_IdCaracteristicaCristal",
                table: "RelacionesCristalCaracteristica",
                column: "IdCaracteristicaCristal");

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesCristalCaracteristica_IdCristal_IdCaracteristicaCristal",
                table: "RelacionesCristalCaracteristica",
                columns: new[] { "IdCristal", "IdCaracteristicaCristal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesCristalVehiculo_IdCristal_IdVehiculo_AnioDesde",
                table: "RelacionesCristalVehiculo",
                columns: new[] { "IdCristal", "IdVehiculo", "AnioDesde" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesCristalVehiculo_IdVehiculo",
                table: "RelacionesCristalVehiculo",
                column: "IdVehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_IdMarcaVehiculo",
                table: "Vehiculos",
                column: "IdMarcaVehiculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelacionesCristalCaracteristica");

            migrationBuilder.DropTable(
                name: "RelacionesCristalVehiculo");

            migrationBuilder.DropTable(
                name: "CaracteristicasCristal");

            migrationBuilder.DropTable(
                name: "Cristales");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "ColoresCristal");

            migrationBuilder.DropTable(
                name: "MarcasCristal");

            migrationBuilder.DropTable(
                name: "OrigenesCristal");

            migrationBuilder.DropTable(
                name: "TiposCristal");

            migrationBuilder.DropTable(
                name: "MarcasVehiculo");
        }
    }
}
