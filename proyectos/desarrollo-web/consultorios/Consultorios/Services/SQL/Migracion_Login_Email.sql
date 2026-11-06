USE [ConsultorioII];
GO

/* ============================================================
   MIGRACIÓN LOGIN
   - Login mediante Email
   - Nombre y Apellido separados
   - NombreUsuario queda temporalmente como columna legacy
   ============================================================ */


/* 1. AGREGAR APELLIDO */

IF COL_LENGTH('dbo.Usuarios', 'Apellido') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD Apellido NVARCHAR(100) NULL;
END;
GO


/* 2. COMPROBAR EMAIL */

IF COL_LENGTH('dbo.Usuarios', 'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD Email NVARCHAR(254) NULL;
END;
GO


/* 3. EMAIL ÚNICO */

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Usuarios_Email'
      AND object_id = OBJECT_ID('dbo.Usuarios')
)
BEGIN
    CREATE UNIQUE INDEX UX_Usuarios_Email
    ON dbo.Usuarios(Email)
    WHERE Email IS NOT NULL;
END;
GO


/* ============================================================
   IMPORTANTE

   NombreUsuario NO se elimina todavía.

   La aplicación dejará de mostrarlo y dejará de utilizarlo
   para iniciar sesión.

   Los usuarios nuevos recibirán internamente un valor legacy
   único en NombreUsuario para mantener compatibilidad con la
   estructura actual.

   Cuando comprobemos que todo funciona, eliminaremos
   definitivamente NombreUsuario de la base.
   ============================================================ */


/* 4. VERIFICACIÓN */

SELECT
    IdUsuario,
    Nombre,
    Apellido,
    Email,
    NombreUsuario,
    IdNivel,
    IdEstado
FROM dbo.Usuarios
ORDER BY IdUsuario;
GO
