# Trading Journal

Aplicación personal para registrar operaciones cerradas y analizar resultados y disciplina. TypeScript, React, validación con Zod y persistencia relacional. Incluye asistencia de IA y componentes de un starter con sus avisos correspondientes.

## Archivos destacados

- [lib/domain.ts](lib/domain.ts): modelos, cálculos, reglas y fechas.
- [lib/csv.ts](lib/csv.ts): importación/exportación y validación.
- [db/schema.ts](db/schema.ts) y [db/repository.ts](db/repository.ts): estructura de datos y acceso preparado.
- [tests](tests): pruebas del dominio y de CSV.

## Ejecutar las pruebas

Requiere Node.js 24 y npm.

```sh
npm ci
npm test
```

En la preparación se ejecutaron las 29 pruebas de dominio y CSV. Estas pruebas no validan toda la interfaz, autenticación o almacenamiento.

## Explorar la aplicación local

```sh
npm run dev
```

La configuración incluida contiene únicamente nombres genéricos de bindings de almacenamiento. Las APIs necesitan una base local con las migraciones de `drizzle/`. El starter usa Cloudflare D1/R2 y un adaptador de autenticación específico de su plataforma; fuera de ella requiere preparar el entorno equivalente. El proveedor simulado del starter debe usarse solo en loopback. No se incluye un despliegue ni se verificó una instalación desde cero.

El proyecto analiza registros; no envía órdenes ni garantiza resultados de trading. Los datos de ejemplo son de demostración.
