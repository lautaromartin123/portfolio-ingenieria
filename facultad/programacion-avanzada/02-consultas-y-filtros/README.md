# consultas y filtros

Práctica de C# / ASP.NET WebForms con una capa BIZ. Requiere Visual Studio con desarrollo web ASP.NET y .NET Framework 4.7.2.

## Qué explorar

- `Avanzada2026/ConsultaAlumno.aspx.cs`: alumnos filtrados por curso.
- `Avanzada2026/ConsultaProductos.aspx.cs`: productos filtrados por estado.
- `BIZ/`: consultas parametrizadas y gestión de recursos.

Las dos pantallas muestran el mismo patrón de consulta en distintos dominios. Las listas vacías y las excepciones que devuelven null necesitan tratamiento más explícito.

## Abrir y preparar

1. Abrir `PracticaFinal2026.slnx` y restaurar paquetes NuGet.
2. Revisar la conexión local de ejemplo en Web.config.
3. Preparar una base local con las tablas/columnas que requieren las consultas. Los scripts originales de la base de clase no estaban disponibles; esta carpeta no incluye una base lista para restaurar.
4. Ejecutar con IIS Express y abrir una de las páginas indicadas.

La compilación de los fuentes no prueba que el markup ni los recorridos con SQL funcionen. Se publica como práctica documentada, con sus límites, no como aplicación completa.
