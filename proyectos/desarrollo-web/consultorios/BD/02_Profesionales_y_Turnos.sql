/* ============================================================
   TP CONSULTORIO - Grupo 05
   Base: ConsultorioII

   PARTE 2 de 2 - PROFESIONALES Y TURNOS
   Autor: Jaremczuk

   Requiere haber ejecutado antes 01_Esquema_Base.sql
   (necesita las tablas Estado, Paciente y Documento).

   Los tipos de dato siguen la convencion real de la base:
     - Nombre / Apellido        VARCHAR(100)
     - Foto                     VARCHAR(500)
     - Descripcion de catalogo  VARCHAR(50)
     - Catalogos (Estado, TipoDocumento) sin IDENTITY, IDs a mano
     - Entidades (Paciente, ObraSocial) con IDENTITY

   Baja logica: nada se borra, se pasa IdEstado a 2 (Inactivo).

   Se puede ejecutar varias veces sin romper nada.
   ============================================================ */


/* ---------- ESPECIALIDAD ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Especialidad')
BEGIN
    CREATE TABLE Especialidad
    (
        IdEspecialidad  INT IDENTITY(1,1)  NOT NULL,
        Nombre          VARCHAR(100)       NOT NULL,

        CONSTRAINT PK_Especialidad PRIMARY KEY (IdEspecialidad)
    );
END
GO


/* ---------- TIPO DE MATRICULA (catalogo) ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TipoMatricula')
BEGIN
    CREATE TABLE TipoMatricula
    (
        IdTipoMatricula  INT          NOT NULL,
        Descripcion      VARCHAR(50)  NOT NULL,

        CONSTRAINT PK_TipoMatricula PRIMARY KEY (IdTipoMatricula)
    );
END
GO


/* ---------- ESTADO DEL TURNO (catalogo) ----------
   Distinta de Estado: Estado sirve para la baja logica de las
   entidades, EstadoTurno describe la situacion de un turno.
   Nombre sin guion bajo para respetar la convencion de la base.
   -------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EstadoTurno')
BEGIN
    CREATE TABLE EstadoTurno
    (
        IdEstadoTurno  INT          NOT NULL,
        Descripcion    VARCHAR(50)  NOT NULL,

        CONSTRAINT PK_EstadoTurno PRIMARY KEY (IdEstadoTurno)
    );
END
GO


/* ---------- PROFESIONAL ----------
   IdDomicilio queda sin FK: la tabla Domicilio la hace otro
   integrante y todavia no existe. Ver el ALTER comentado al final.
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Profesional')
BEGIN
    CREATE TABLE Profesional
    (
        IdProfesional   INT IDENTITY(1,1)  NOT NULL,
        Nombre          VARCHAR(100)       NOT NULL,
        Apellido        VARCHAR(100)       NOT NULL,
        Foto            VARCHAR(500)           NULL,

        IdEspecialidad  INT                NOT NULL,
        IdEstado        INT                NOT NULL
            CONSTRAINT DF_Profesional_IdEstado DEFAULT (1),
        IdDomicilio     INT                    NULL,

        CONSTRAINT PK_Profesional PRIMARY KEY (IdProfesional),

        CONSTRAINT FK_Profesional_Especialidad
            FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad (IdEspecialidad),

        CONSTRAINT FK_Profesional_Estado
            FOREIGN KEY (IdEstado) REFERENCES Estado (IdEstado)
    );
END
GO


/* ---------- MATRICULA ----------
   Un profesional puede tener mas de una matricula.
   ------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Matricula')
BEGIN
    CREATE TABLE Matricula
    (
        IdMatricula       INT IDENTITY(1,1)  NOT NULL,
        Numero            VARCHAR(50)        NOT NULL,
        EntidadEmisora    VARCHAR(100)           NULL,
        FechaVencimiento  DATE                   NULL,

        IdProfesional     INT                NOT NULL,
        IdTipoMatricula   INT                NOT NULL,
        IdEstado          INT                NOT NULL
            CONSTRAINT DF_Matricula_IdEstado DEFAULT (1),

        CONSTRAINT PK_Matricula PRIMARY KEY (IdMatricula),

        CONSTRAINT FK_Matricula_Profesional
            FOREIGN KEY (IdProfesional) REFERENCES Profesional (IdProfesional),

        CONSTRAINT FK_Matricula_TipoMatricula
            FOREIGN KEY (IdTipoMatricula) REFERENCES TipoMatricula (IdTipoMatricula),

        CONSTRAINT FK_Matricula_Estado
            FOREIGN KEY (IdEstado) REFERENCES Estado (IdEstado)
    );
END
GO


/* ---------- AGENDA ----------
   Horario semanal de atencion de cada profesional.
   DiaSemana: 1 = Lunes ... 7 = Domingo
   ------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Agenda')
BEGIN
    CREATE TABLE Agenda
    (
        IdAgenda          INT IDENTITY(1,1)  NOT NULL,

        IdProfesional     INT                NOT NULL,
        DiaSemana         TINYINT            NOT NULL,
        HoraInicio        TIME               NOT NULL,
        HoraFin           TIME               NOT NULL,
        DuracionMinutos   INT                NOT NULL
            CONSTRAINT DF_Agenda_DuracionMinutos DEFAULT (30),

        -- 1 = atiende con turno, 0 = por orden de llegada
        AtencionConTurno  BIT                NOT NULL
            CONSTRAINT DF_Agenda_AtencionConTurno DEFAULT (1),

        IdEstado          INT                NOT NULL
            CONSTRAINT DF_Agenda_IdEstado DEFAULT (1),

        CONSTRAINT PK_Agenda PRIMARY KEY (IdAgenda),

        CONSTRAINT FK_Agenda_Profesional
            FOREIGN KEY (IdProfesional) REFERENCES Profesional (IdProfesional),

        CONSTRAINT FK_Agenda_Estado
            FOREIGN KEY (IdEstado) REFERENCES Estado (IdEstado),

        CONSTRAINT CK_Agenda_DiaSemana CHECK (DiaSemana BETWEEN 1 AND 7),
        CONSTRAINT CK_Agenda_Horario   CHECK (HoraFin > HoraInicio)
    );
END
GO


/* ---------- TURNO ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Turno')
BEGIN
    CREATE TABLE Turno
    (
        IdTurno          INT IDENTITY(1,1)  NOT NULL,
        FechaHora        DATETIME           NOT NULL,
        DuracionMinutos  INT                NOT NULL
            CONSTRAINT DF_Turno_DuracionMinutos DEFAULT (30),
        Motivo           VARCHAR(300)           NULL,
        Observaciones    VARCHAR(500)           NULL,

        IdPaciente       INT                NOT NULL,
        IdProfesional    INT                NOT NULL,
        IdEstadoTurno    INT                NOT NULL
            CONSTRAINT DF_Turno_IdEstadoTurno DEFAULT (1),

        CONSTRAINT PK_Turno PRIMARY KEY (IdTurno),

        CONSTRAINT FK_Turno_Paciente
            FOREIGN KEY (IdPaciente) REFERENCES Paciente (IdPaciente),

        CONSTRAINT FK_Turno_Profesional
            FOREIGN KEY (IdProfesional) REFERENCES Profesional (IdProfesional),

        CONSTRAINT FK_Turno_EstadoTurno
            FOREIGN KEY (IdEstadoTurno) REFERENCES EstadoTurno (IdEstadoTurno)
    );

    -- La consulta mas usada del ABM: turnos de un profesional por fecha
    CREATE INDEX IX_Turno_Profesional_Fecha
        ON Turno (IdProfesional, FechaHora);
END
GO


/* ---------- FK que estaba pendiente ----------
   Documento.IdProfesional ya existia en la base Test pero sin
   clave foranea, porque Profesional no existia. Ahora si.
   -------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Documento_Profesional')
BEGIN
    ALTER TABLE Documento
        ADD CONSTRAINT FK_Documento_Profesional
        FOREIGN KEY (IdProfesional) REFERENCES Profesional (IdProfesional);
END
GO


/* ============================================================
   DATOS DE CATALOGO
   Sin estos registros el ABM no se puede probar.
   ============================================================ */

IF NOT EXISTS (SELECT 1 FROM Especialidad)
BEGIN
    INSERT INTO Especialidad (Nombre) VALUES
        ('Clinica Medica'),
        ('Cardiologia'),
        ('Pediatria'),
        ('Traumatologia'),
        ('Dermatologia');
END
GO

IF NOT EXISTS (SELECT 1 FROM TipoMatricula)
BEGIN
    INSERT INTO TipoMatricula (IdTipoMatricula, Descripcion) VALUES
        (1, 'Nacional'),
        (2, 'Provincial');
END
GO

IF NOT EXISTS (SELECT 1 FROM EstadoTurno)
BEGIN
    INSERT INTO EstadoTurno (IdEstadoTurno, Descripcion) VALUES
        (1, 'Pendiente'),
        (2, 'Confirmado'),
        (3, 'Cancelado'),
        (4, 'Atendido'),
        (5, 'Ausente');
END
GO


/* ============================================================
   PENDIENTE: ejecutar cuando exista la tabla Domicilio.
   La hace otro integrante del grupo.

   ALTER TABLE Profesional
       ADD CONSTRAINT FK_Profesional_Domicilio
       FOREIGN KEY (IdDomicilio) REFERENCES Domicilio (IdDomicilio);
   ============================================================ */
