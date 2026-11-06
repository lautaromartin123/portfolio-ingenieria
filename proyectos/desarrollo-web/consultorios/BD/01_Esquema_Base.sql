/* ============================================================
   TP CONSULTORIO - Grupo 05
   Base: ConsultorioII

   PARTE 1 de 2 - TABLAS BASE (de otros integrantes)

   Estas tablas NO son inventadas: son una copia exacta de las
   que ya existen en la base "Test", donde el grupo estuvo
   trabajando por error. Se migran a ConsultorioII, que es la
   base correcta y estaba vacia.

   Duenios:
     Paciente / Estado / ObraSocial / TipoDocumento / Documento -> Abalos
     Usuarios                                                   -> Alvarez Reynoso

   Orden de ejecucion: este archivo PRIMERO, despues 02.

   Se puede ejecutar varias veces: cada bloque verifica si la
   tabla ya existe antes de crearla.
   ============================================================ */


/* ---------- ESTADO (catalogo, IDs manuales) ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Estado')
BEGIN
    CREATE TABLE Estado
    (
        IdEstado     INT          NOT NULL,
        Descripcion  VARCHAR(50)  NOT NULL,

        CONSTRAINT PK_Estado PRIMARY KEY (IdEstado)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Estado)
BEGIN
    INSERT INTO Estado (IdEstado, Descripcion) VALUES
        (1, 'Activo'),
        (2, 'Inactivo');
END
GO


/* ---------- TIPO DOCUMENTO (catalogo, IDs manuales) ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TipoDocumento')
BEGIN
    CREATE TABLE TipoDocumento
    (
        IdTipoDocumento  INT          NOT NULL,
        Descripcion      VARCHAR(50)  NOT NULL,

        CONSTRAINT PK_TipoDocumento PRIMARY KEY (IdTipoDocumento)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM TipoDocumento)
BEGIN
    INSERT INTO TipoDocumento (IdTipoDocumento, Descripcion) VALUES
        (1, 'DNI'),
        (2, 'Pasaporte'),
        (3, 'Otro');
END
GO


/* ---------- OBRA SOCIAL ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ObraSocial')
BEGIN
    CREATE TABLE ObraSocial
    (
        IdObraSocial  INT IDENTITY(1,1)  NOT NULL,
        Nombre        VARCHAR(100)       NOT NULL,

        CONSTRAINT PK_ObraSocial PRIMARY KEY (IdObraSocial)
    );
END
GO


/* ---------- PACIENTE ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Paciente')
BEGIN
    CREATE TABLE Paciente
    (
        IdPaciente        INT IDENTITY(1,1)  NOT NULL,
        Nombre            VARCHAR(100)       NOT NULL,
        Apellido          VARCHAR(100)       NOT NULL,
        Telefono          VARCHAR(30)            NULL,
        Foto              VARCHAR(500)           NULL,
        NumeroObraSocial  VARCHAR(100)           NULL,
        IdObraSocial      INT                    NULL,
        IdEstado          INT                NOT NULL
            CONSTRAINT DF_Paciente_IdEstado DEFAULT (1),
        IdDomicilio       INT                    NULL,

        CONSTRAINT PK_Paciente PRIMARY KEY (IdPaciente),

        CONSTRAINT FK_Paciente_ObraSocial
            FOREIGN KEY (IdObraSocial) REFERENCES ObraSocial (IdObraSocial),

        CONSTRAINT FK_Paciente_Estado
            FOREIGN KEY (IdEstado) REFERENCES Estado (IdEstado)
    );
END
GO


/* ---------- DOCUMENTO ----------
   IdProfesional ya venia previsto en la base Test, pero sin
   clave foranea porque la tabla Profesional no existia.
   La FK se agrega al final del archivo 02.
   ------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Documento')
BEGIN
    CREATE TABLE Documento
    (
        IdDocumento      INT IDENTITY(1,1)  NOT NULL,
        NumeroDocumento  VARCHAR(50)        NOT NULL,
        IdTipoDocumento  INT                NOT NULL,
        IdPaciente       INT                    NULL,
        IdProfesional    INT                    NULL,

        CONSTRAINT PK_Documento PRIMARY KEY (IdDocumento),

        CONSTRAINT FK_Documento_TipoDocumento
            FOREIGN KEY (IdTipoDocumento) REFERENCES TipoDocumento (IdTipoDocumento),

        CONSTRAINT FK_Documento_Paciente
            FOREIGN KEY (IdPaciente) REFERENCES Paciente (IdPaciente)
    );
END
GO


/* ---------- USUARIOS (Alvarez Reynoso) ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios
    (
        IdUsuario      INT IDENTITY(1,1)  NOT NULL,
        NombreUsuario  NVARCHAR(50)       NOT NULL,
        PasswordHash   VARBINARY(64)      NOT NULL,
        PasswordSalt   VARBINARY(32)      NOT NULL,
        Rol            NVARCHAR(20)       NOT NULL,
        Activo         BIT                NOT NULL
            CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        FechaCreacion  DATETIME           NOT NULL
            CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (GETDATE()),

        CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),

        CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),

        CONSTRAINT CK_Usuarios_Rol
            CHECK (Rol IN (N'ADMIN', N'MEDICO', N'PACIENTE'))
    );
END
GO
