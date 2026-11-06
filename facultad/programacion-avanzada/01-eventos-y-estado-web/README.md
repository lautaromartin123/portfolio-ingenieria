# eventos y estado web

Práctica de C# / ASP.NET WebForms con una capa BIZ. Requiere Visual Studio con desarrollo web ASP.NET y .NET Framework 4.8.

## Qué explorar

- `Eeventos/DemoEstado.aspx.cs`: Session, ViewState y PostBack.
- `Eeventos/EjemploCookie.aspx.cs`: preferencia en cookie.
- `Eeventos/DropDownAnidado.aspx.cs`: listas padre/hijo.
- `Eeventos/Repeater.aspx.cs` y `producto.aspx.cs`: listado, búsqueda y detalle.

En la copia pública se corrigieron nombres de métodos/clave de conexión, sintaxis SQL y eventos de Final2026; el cálculo de edad considera si ya ocurrió el cumpleaños. Las fuentes originales se conservaron aparte. Quedan casos límite como identificadores inválidos, errores de consulta y resultados vacíos. El ejemplo JavaScript externo todavía requiere revisar cómo se obtiene el ClientID.

## Abrir y preparar

1. Abrir `Eeventos.slnx` y restaurar paquetes NuGet.
2. Revisar la conexión local de ejemplo en Web.config.
3. Preparar una base local con las tablas/columnas que requieren las consultas. Los scripts originales de la base de clase no estaban disponibles; esta carpeta no incluye una base lista para restaurar.
4. Ejecutar con IIS Express y abrir una de las páginas indicadas.

La compilación de los fuentes no prueba que el markup ni los recorridos con SQL funcionen. Se publica como práctica documentada, con sus límites, no como aplicación completa.
