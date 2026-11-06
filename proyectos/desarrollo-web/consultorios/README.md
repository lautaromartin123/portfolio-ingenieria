# Consultorios

Proyecto académico grupal con C#, ASP.NET WebForms y SQL Server. Reúne pacientes, profesionales, turnos, agenda y permisos. Se publica una copia seleccionada, sin conexiones reales ni historial del repositorio de trabajo.

## Archivos para explorar

| Concepto | Código |
| --- | --- |
| Entidades | [BIZ/Modelo](BIZ/Modelo) |
| Acceso a datos | [BIZ/Data](BIZ/Data) |
| Solapamiento de turnos | [TurnoNegocio.cs](BIZ/Data/TurnoNegocio.cs) |
| Sesión y permisos | [Seguridad.cs](Consultorios/Security/Seguridad.cs) |
| Contraseñas con hash y salt | [PasswordHelper.cs](Consultorios/Security/PasswordHelper.cs) |
| Esquema relacional | [BD](BD) |

## Preparación

1. Abrir `TP-CONSULTORIO.slnx` con Visual Studio y desarrollo web ASP.NET / .NET Framework 4.8.
2. Restaurar los paquetes NuGet declarados en los proyectos.
3. Crear una base local vacía `PortfolioDemo` en SQL Server o LocalDB.
4. Revisar y ejecutar en esa base los scripts numerados de `BD/` en orden ascendente. El esquema puede requerir ajustes porque refleja la evolución de un trabajo grupal; no hay un entorno de integración certificado.
5. Revisar la conexión de ejemplo en `Consultorios/Web.config` y ejecutar con IIS Express.

Los servicios opcionales de correo requieren variables de entorno propias. La integración experimental con Ollama requiere un servicio local; no se incluye una cuenta ni infraestructura externa.

## Estado y límites

Es un trabajo académico en evolución. La publicación no certifica que cada flujo, migración o permiso funcione de punta a punta. No se conectó a la base de la facultad durante la preparación del portfolio. Los módulos de IA son experimentales. Las atribuciones de integrantes presentes en los fuentes se conservan; no se reclama autoría individual de todo el sistema.
