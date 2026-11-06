# Importador y normalizador de catálogos Excel

Prototipo C# con ASP.NET Core, ClosedXML y EF Core. Detecta encabezados, lee filas, normaliza modelos/tipos de cristal y muestra resultados de validación y una vista previa.

- [ImportacionExcelService.cs](Services/ImportacionExcelService.cs): transformación.
- [ImportacionController.cs](Controllers/ImportacionController.cs): archivo, catálogos y vista previa.
- [StockCristalesContext.cs](Data/StockCristalesContext.cs): relaciones e índices.

Requiere .NET 10 y SQL Server / LocalDB. Abrir `StockCristales.slnx`, restaurar paquetes y aplicar las migraciones a una base local nueva según `appsettings.json`. Ejecutar y navegar a `/Importacion`. No se incluye ningún catálogo comercial original; para probar, crear un Excel ficticio con columnas `Marca`, `Código` y `Modelo`, y los campos complementarios documentados en el servicio.

Alcance: el controlador muestra hasta 20 filas de vista previa; no guarda el catálogo de productos importados. No es un sistema de stock terminado. Esta copia no incluye bibliotecas frontend restaurables ni una verificación de ejecución integral.
