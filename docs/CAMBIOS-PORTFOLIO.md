# Preparación de la copia pública

- Se crearon índices por área y README para cada muestra.
- Se sustituyeron conexiones reales por ejemplos locales con autenticación integrada.
- Se excluyeron credenciales, estado de ejecución, dependencias restaurables, respaldos y conversaciones privadas.
- Milagros está representado por un texto nuevo de capacidades generales, sin copiar código, datos ni despliegue.
- Se mantuvieron separadas las tres soluciones de C#.
- En la copia de Eventos se corrigieron claves de conexión, Read/Open, consultas y eventos de Final2026. También se corrigió el cálculo del cumpleaños.
- En Avanzada2026 se excluyó una variante no incluida en los csproj que dependía de un método inexistente.
- SOL conserva la corrección de escala realizada en el proyecto original. Sus campos de notificaciones en esta copia se leen de variables de entorno.
- Las guías Linux reproducibles son documentación nueva; los fuentes C y Python proceden de las prácticas presentes en WSL.

Las correcciones de preparación no reescriben los proyectos originales de la facultad. Se conservan los límites académicos de sus ejemplos.

## Navegación simplificada

- Las siete simulaciones tienen carpetas individuales dentro de experimentos, con sus nombres originales de archivo y enlaces directos desde la portada.
- Los siete fuentes se trasladaron sin cambiar su contenido; se comprobó su identidad con las versiones anteriores.
- Se agregaron índices en los niveles de proyectos y facultad, y accesos a pantallas, lógica, scripts y pruebas.
- Los ejemplos de instalación Python usan entornos virtuales explícitos para evitar modificar un intérprete administrado por uv.
- Los lanzadores de Windows se ubican en la carpeta correcta antes de ejecutar, y las simulaciones comparten un menú de selección.

## Arranque local del diario de trading

- Se incorporó la hoja de estilos shadcn que faltaba en la copia, con su licencia.
- El inicio prepara automáticamente las migraciones pendientes de D1 en almacenamiento local.
- Se agregó ABRIR_JOURNAL.cmd y se comprobaron el arranque web, la demo y el acceso a la base con la identidad local de prueba.
- La auditoría de dependencias informó alertas que necesitan revisión específica. Las pruebas funcionales y el arranque no sustituyen esa revisión.
