USE [ConsultorioII];
GO

/* ============================================
   TABLA NIVEL
   ============================================ */

IF OBJECT_ID('dbo.Nivel', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Nivel
    (
        IdNivel INT IDENTITY(1,1) NOT NULL,
        Descripcion VARCHAR(50) NOT NULL,

        CONSTRAINT PK_Nivel
            PRIMARY KEY (IdNivel),

        CONSTRAINT UQ_Nivel_Descripcion
            UNIQUE (Descripcion)
    );
END;
GO


/* ============================================
   NIVELES
   ============================================ */

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Nivel
    WHERE Descripcion = 'ADMIN'
)
    INSERT INTO dbo.Nivel (Descripcion)
    VALUES ('ADMIN');

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Nivel
    WHERE Descripcion = 'MEDICO'
)
    INSERT INTO dbo.Nivel (Descripcion)
    VALUES ('MEDICO');

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Nivel
    WHERE Descripcion = 'PACIENTE'
)
    INSERT INTO dbo.Nivel (Descripcion)
    VALUES ('PACIENTE');
GO


/* ============================================
   TABLA URL
   ============================================ */

IF OBJECT_ID('dbo.URL', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.URL
    (
        IdUrl INT IDENTITY(1,1) NOT NULL,
        Descripcion VARCHAR(150) NOT NULL,

        CONSTRAINT PK_URL
            PRIMARY KEY (IdUrl),

        CONSTRAINT UQ_URL_Descripcion
            UNIQUE (Descripcion)
    );
END;
GO


/* ============================================
   URLS INICIALES
   ============================================ */

IF NOT EXISTS (
    SELECT 1 FROM dbo.URL
    WHERE Descripcion = 'PanelAdmin.aspx'
)
    INSERT INTO dbo.URL (Descripcion)
    VALUES ('PanelAdmin.aspx');

IF NOT EXISTS (
    SELECT 1 FROM dbo.URL
    WHERE Descripcion = 'PanelMedico.aspx'
)
    INSERT INTO dbo.URL (Descripcion)
    VALUES ('PanelMedico.aspx');

IF NOT EXISTS (
    SELECT 1 FROM dbo.URL
    WHERE Descripcion = 'PanelPaciente.aspx'
)
    INSERT INTO dbo.URL (Descripcion)
    VALUES ('PanelPaciente.aspx');
GO


/* ============================================
   TABLA NIVEL_URL
   ============================================ */

IF OBJECT_ID('dbo.Nivel_URL', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Nivel_URL
    (
        IdNivel INT NOT NULL,
        IdUrl INT NOT NULL,
        IdEstado INT NOT NULL,

        CONSTRAINT PK_Nivel_URL
            PRIMARY KEY (IdNivel, IdUrl),

        CONSTRAINT FK_Nivel_URL_Nivel
            FOREIGN KEY (IdNivel)
            REFERENCES dbo.Nivel(IdNivel),

        CONSTRAINT FK_Nivel_URL_URL
            FOREIGN KEY (IdUrl)
            REFERENCES dbo.URL(IdUrl),

        CONSTRAINT FK_Nivel_URL_Estado
            FOREIGN KEY (IdEstado)
            REFERENCES dbo.Estado(IdEstado)
    );
END;
GO


/* ============================================
   NUEVAS COLUMNAS EN USUARIOS
   ============================================ */

IF COL_LENGTH('dbo.Usuarios', 'IdNivel') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD IdNivel INT NULL;
END;
GO

IF COL_LENGTH('dbo.Usuarios', 'IdEstado') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD IdEstado INT NULL;
END;
GO


/* ============================================
   MIGRACIÓN DE DATOS EXISTENTES
   ============================================ */

UPDATE U
SET U.IdNivel = N.IdNivel
FROM dbo.Usuarios U
INNER JOIN dbo.Nivel N
    ON N.Descripcion = U.Rol
WHERE U.IdNivel IS NULL;
GO

UPDATE dbo.Usuarios
SET IdEstado =
    CASE
        WHEN Activo = 1 THEN 1
        ELSE 2
    END
WHERE IdEstado IS NULL;
GO


/* ============================================
   PERMISOS INICIALES
   ============================================ */

INSERT INTO dbo.Nivel_URL
    (IdNivel, IdUrl, IdEstado)
SELECT
    N.IdNivel,
    U.IdUrl,
    1
FROM dbo.Nivel N
CROSS JOIN dbo.URL U
WHERE N.Descripcion = 'ADMIN'
  AND U.Descripcion = 'PanelAdmin.aspx'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Nivel_URL NU
      WHERE NU.IdNivel = N.IdNivel
        AND NU.IdUrl = U.IdUrl
  );

INSERT INTO dbo.Nivel_URL
    (IdNivel, IdUrl, IdEstado)
SELECT
    N.IdNivel,
    U.IdUrl,
    1
FROM dbo.Nivel N
CROSS JOIN dbo.URL U
WHERE N.Descripcion = 'MEDICO'
  AND U.Descripcion = 'PanelMedico.aspx'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Nivel_URL NU
      WHERE NU.IdNivel = N.IdNivel
        AND NU.IdUrl = U.IdUrl
  );

INSERT INTO dbo.Nivel_URL
    (IdNivel, IdUrl, IdEstado)
SELECT
    N.IdNivel,
    U.IdUrl,
    1
FROM dbo.Nivel N
CROSS JOIN dbo.URL U
WHERE N.Descripcion = 'PACIENTE'
  AND U.Descripcion = 'PanelPaciente.aspx'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Nivel_URL NU
      WHERE NU.IdNivel = N.IdNivel
        AND NU.IdUrl = U.IdUrl
  );
GO
