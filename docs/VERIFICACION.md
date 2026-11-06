# Verificaciones

Preparación: 6 de noviembre de 2026. Este archivo distingue código revisado, pruebas y límites. No certifica que todos los proyectos funcionen completos.

| Muestra | Comprobación | Límite |
| --- | --- | --- |
| Trading Journal | 29 pruebas de dominio y CSV aprobadas | No comprueba toda la aplicación web ni el proveedor de autenticación |
| SOL | Cinco valores de frontera, NaN y 582 filas válidas del CSV | Función aislada, sin soportes/resistencias ni bucle de red |
| C de WSL | Sintaxis con GCC y advertencias | No es una auditoría de robustez de llamadas al sistema |
| Python WSL | Análisis sintáctico | No valida todos los modelos ni la interfaz gráfica |
| Tres prácticas C# | Compilación de los fuentes declarados en sus proyectos | No sustituye MSBuild, precompilación ASPX o pruebas con SQL Server |
| Consultorios | Fuentes BIZ y code-behind compilados; entidades, consultas y reglas revisadas | Integración y migraciones completas pendientes |
| Importador Excel | Revisión de servicio, controlador y modelo | Ejecución integral pendiente |
| Milagros | Caso de estudio escrito para el portfolio | Sin copia del sistema original |

Los resultados finales de preparación y las correcciones de la copia se registran en [CAMBIOS-PORTFOLIO.md](CAMBIOS-PORTFOLIO.md).

En la copia final compilaron los ocho proyectos C# (BIZ y web de las tres prácticas y Consultorios). Los 20 archivos de configuración XML se analizaron sin errores. La revisión de enlaces Markdown relativos no encontró destinos ausentes. Las pruebas del diario se ejecutaron con dependencias ya instaladas, no mediante una instalación desde cero.
