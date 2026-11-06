/* Amplia el circuito de solicitudes de medicos.
   Conserva la especialidad principal de las pantallas existentes.
   Los datos actuales se copian a las relaciones nuevas sin eliminarlos. */
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.SolicitudModificacionMedico', 'U') IS NULL
        THROW 50001, 'Primero se debe ejecutar el script 05 de solicitudes de medicos.', 1;

    IF OBJECT_ID('dbo.ProfesionalEspecialidad', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProfesionalEspecialidad
        (
            IdProfesional INT NOT NULL REFERENCES dbo.Profesional(IdProfesional),
            IdEspecialidad INT NOT NULL REFERENCES dbo.Especialidad(IdEspecialidad),
            CONSTRAINT PK_ProfesionalEspecialidad PRIMARY KEY (IdProfesional, IdEspecialidad)
        );
    END;

    INSERT dbo.ProfesionalEspecialidad (IdProfesional, IdEspecialidad)
    SELECT P.IdProfesional, P.IdEspecialidad FROM dbo.Profesional P
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.ProfesionalEspecialidad PE
        WHERE PE.IdProfesional=P.IdProfesional AND PE.IdEspecialidad=P.IdEspecialidad
    );

    IF OBJECT_ID('dbo.SolicitudModificacionMedicoEspecialidad', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SolicitudModificacionMedicoEspecialidad
        (
            IdSolicitud INT NOT NULL REFERENCES dbo.SolicitudModificacionMedico(IdSolicitud),
            IdEspecialidad INT NOT NULL REFERENCES dbo.Especialidad(IdEspecialidad),
            CONSTRAINT PK_SolicitudModificacionMedicoEspecialidad PRIMARY KEY (IdSolicitud, IdEspecialidad)
        );
    END;

    INSERT dbo.SolicitudModificacionMedicoEspecialidad (IdSolicitud, IdEspecialidad)
    SELECT D.IdSolicitud,D.IdEspecialidad FROM dbo.SolicitudModificacionMedico D
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.SolicitudModificacionMedicoEspecialidad DE
        WHERE DE.IdSolicitud=D.IdSolicitud AND DE.IdEspecialidad=D.IdEspecialidad
    );

    IF COL_LENGTH('dbo.SolicitudModificacionMedico', 'NumeroMatricula') IS NULL
        ALTER TABLE dbo.SolicitudModificacionMedico ADD NumeroMatricula VARCHAR(50) NULL;
    IF COL_LENGTH('dbo.SolicitudModificacionMedico', 'IdTipoMatricula') IS NULL
        ALTER TABLE dbo.SolicitudModificacionMedico ADD IdTipoMatricula INT NULL
            CONSTRAINT FK_SolicitudMedico_TipoMatricula REFERENCES dbo.TipoMatricula(IdTipoMatricula);
    IF COL_LENGTH('dbo.SolicitudModificacionMedico', 'EntidadEmisora') IS NULL
        ALTER TABLE dbo.SolicitudModificacionMedico ADD EntidadEmisora VARCHAR(100) NULL;
    IF COL_LENGTH('dbo.SolicitudModificacionMedico', 'FechaVencimiento') IS NULL
        ALTER TABLE dbo.SolicitudModificacionMedico ADD FechaVencimiento DATE NULL;

    IF OBJECT_ID('dbo.HistorialMatricula', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.HistorialMatricula
        (
            IdHistorial INT IDENTITY CONSTRAINT PK_HistorialMatricula PRIMARY KEY,
            IdMatricula INT NOT NULL REFERENCES dbo.Matricula(IdMatricula),
            IdUsuarioAdmin INT NOT NULL REFERENCES dbo.Usuarios(IdUsuario),
            IdSolicitud INT NULL REFERENCES dbo.SolicitudModificacion(IdSolicitud),
            FechaCambio DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
            Operacion VARCHAR(20) NOT NULL,
            Motivo NVARCHAR(500) NOT NULL,
            NumeroAnterior VARCHAR(50) NULL,
            NumeroNuevo VARCHAR(50) NOT NULL,
            IdTipoAnterior INT NULL,
            IdTipoNuevo INT NOT NULL,
            EntidadAnterior VARCHAR(100) NULL,
            EntidadNueva VARCHAR(100) NULL,
            VencimientoAnterior DATE NULL,
            VencimientoNuevo DATE NULL,
            IdEstadoAnterior INT NULL,
            IdEstadoNuevo INT NOT NULL
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Matricula') AND name='IX_Matricula_Profesional')
        CREATE INDEX IX_Matricula_Profesional ON dbo.Matricula(IdProfesional);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
