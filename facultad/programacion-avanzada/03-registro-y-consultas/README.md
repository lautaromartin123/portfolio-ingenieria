# registro y consultas

Práctica de C# / ASP.NET WebForms con una capa BIZ. Requiere Visual Studio con desarrollo web ASP.NET y .NET Framework 4.7.2.

## Qué explorar

- `Avanzada2026/Ejercicio2.aspx.cs`: productos por categoría.
- `Avanzada2026/Ejercicio3.aspx.cs`: usuarios por estado.
- `Avanzada2026/CrearCuenta.aspx.cs`: formulario y lectura de una imagen.

Este es un ejercicio histórico. El código de usuario guarda/compara contraseñas sin hash y el login no establece una sesión protegida. No debe reutilizarse como autenticación de producción. Recupera.aspx es una pantalla sin flujo implementado. La variante Ejercicio.aspx/categoria.cs, no incluida en los proyectos, se excluyó de esta copia.

## Abrir y preparar

1. Abrir `Avanzada2026.slnx` y restaurar paquetes NuGet.
2. Revisar la conexión local de ejemplo en Web.config.
3. Preparar una base local con las tablas/columnas que requieren las consultas. Los scripts originales de la base de clase no estaban disponibles; esta carpeta no incluye una base lista para restaurar.
4. Ejecutar con IIS Express y abrir una de las páginas indicadas.

La compilación de los fuentes no prueba que el markup ni los recorridos con SQL funcionen. Se publica como práctica documentada, con sus límites, no como aplicación completa.
