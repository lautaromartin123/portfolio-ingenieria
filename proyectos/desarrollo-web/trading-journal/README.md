# Trading Journal

Aplicación personal para registrar operaciones cerradas y analizar resultados y disciplina. TypeScript, React, validación con Zod y persistencia relacional. Incluye asistencia de IA y componentes de un starter con sus avisos correspondientes.

## Archivos destacados

- [lib/domain.ts](lib/domain.ts): modelos, cálculos, reglas y fechas.
- [lib/csv.ts](lib/csv.ts): importación/exportación y validación.
- [db/schema.ts](db/schema.ts) y [db/repository.ts](db/repository.ts): estructura de datos y acceso preparado.
- [tests](tests): pruebas del dominio y de CSV.

## Ejecutar las pruebas

Requiere Node.js 24 y npm disponibles en la terminal. Después de instalar Node, cerrar y volver a abrir CMD. Comprobar con `node --version` y `npm --version`.

**En Windows:** abrir la carpeta de este proyecto y hacer doble clic en [INSTALAR_Y_PROBAR.cmd](INSTALAR_Y_PROBAR.cmd). El archivo entra en la carpeta correcta, instala las dependencias y ejecuta las pruebas.

Para hacerlo manualmente, los comandos siguientes se ejecutan **dentro de la carpeta trading-journal**, donde están package.json y package-lock.json. En CMD se cambia de carpeta con `cd /d "RUTA_DE_LA_CARPETA_TRADING_JOURNAL"`; ejecutar `npm ci` desde la carpeta personal no instala este proyecto.

```sh
npm ci
npm test
```

Se comprobó una instalación con `npm ci` en Windows usando Node.js 24.22.2 y npm 11.19.1; después pasaron las 29 pruebas de dominio y CSV. Estas pruebas no validan toda la interfaz, autenticación o almacenamiento.

## Explorar la aplicación local

En Windows, hacer doble clic en [ABRIR_JOURNAL.cmd](ABRIR_JOURNAL.cmd). El lanzador se ubica en la carpeta del proyecto y prepara las dependencias si falta vinext. La instalación inicial puede tardar varios minutos; dejarla terminar antes de abrir la aplicación.

También se puede iniciar desde la terminal, dentro de esta carpeta:

```sh
npm run dev
```

El comando prepara automáticamente las migraciones pendientes en la base de datos local y arranca el servidor. Abrir la dirección que aparece como **Local**, normalmente http://localhost:5173/. Mantener la terminal abierta mientras se utiliza la aplicación; Ctrl+C detiene el servidor.

- **Explorar demo** muestra datos ficticios y funciona sin entrar a una cuenta.
- En desarrollo local, **Iniciar sesión con ChatGPT** usa una identidad de prueba del starter. Es una sesión local de demostración; no representa una conexión con una cuenta real de ChatGPT.
- Los datos locales se guardan en `.wrangler/state`, carpeta excluida de Git. `npm run db:local` permite preparar la base por separado; solo aplica migraciones pendientes.

Se comprobó el arranque, la carga de la demo y la lectura de la base tras el acceso local. La configuración incluye nombres genéricos de bindings y una base de desarrollo. El proveedor simulado debe utilizarse solo en loopback. Un despliegue requiere almacenamiento y autenticación reales; los demás recorridos completos siguen pendientes de validación de integración.

El proyecto analiza registros; no envía órdenes ni garantiza resultados de trading. Los datos de ejemplo son de demostración.
