/* Agenda personal, solicitudes de horarios y registro de atenciones.
   No modifica turnos ni horarios existentes. Ejecutar despues de 05 y 06. */
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF COL_LENGTH('dbo.Agenda','VersionAgenda') IS NULL
        ALTER TABLE dbo.Agenda ADD VersionAgenda ROWVERSION;
    IF COL_LENGTH('dbo.Turno','CoberturaRegistrada') IS NULL
        ALTER TABLE dbo.Turno ADD CoberturaRegistrada BIT NOT NULL DEFAULT 0,
            ObraSocialTurno VARCHAR(100) NULL, NumeroAfiliadoTurno VARCHAR(100) NULL,
            IdUsuarioCreador INT NULL REFERENCES dbo.Usuarios(IdUsuario),
            IdUsuarioAtencion INT NULL REFERENCES dbo.Usuarios(IdUsuario),
            FechaAtencionUTC DATETIME2 NULL;

    IF OBJECT_ID('dbo.SolicitudModificacionAgenda','U') IS NULL
        CREATE TABLE dbo.SolicitudModificacionAgenda
        (
            IdSolicitud INT PRIMARY KEY REFERENCES dbo.SolicitudModificacion(IdSolicitud),
            IdAgenda INT NULL REFERENCES dbo.Agenda(IdAgenda),
            VersionAgenda VARBINARY(8) NULL,
            DiaSemana TINYINT NOT NULL CHECK (DiaSemana BETWEEN 1 AND 7),
            HoraInicio TIME NOT NULL, HoraFin TIME NOT NULL,
            DuracionMinutos INT NOT NULL CHECK (DuracionMinutos BETWEEN 5 AND 240),
            DarDeBaja BIT NOT NULL DEFAULT 0,
            CHECK (HoraFin>HoraInicio),
            CHECK (DarDeBaja=0 OR IdAgenda IS NOT NULL)
        );
    IF OBJECT_ID('dbo.HistorialAtencion','U') IS NULL
        CREATE TABLE dbo.HistorialAtencion
        (
            IdHistorial INT IDENTITY PRIMARY KEY,
            IdTurno INT NOT NULL REFERENCES dbo.Turno(IdTurno),
            IdUsuarioMedico INT NOT NULL REFERENCES dbo.Usuarios(IdUsuario),
            FechaUTC DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
            ObservacionAnterior VARCHAR(500) NULL,
            ObservacionNueva VARCHAR(500) NOT NULL
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Turno') AND name='IX_Turno_Paciente_Fecha')
        CREATE INDEX IX_Turno_Paciente_Fecha ON dbo.Turno(IdPaciente,FechaHora);

    DECLARE @Accesos TABLE(Nivel VARCHAR(50),Pagina VARCHAR(150));
    INSERT @Accesos VALUES ('MEDICO','MiAgendaMedico.aspx'),
        ('PACIENTE','MisTurnosPaciente.aspx'),('ADMIN','SolicitudesAgenda.aspx');
    INSERT dbo.URL(Descripcion)
    SELECT DISTINCT A.Pagina FROM @Accesos A
    WHERE NOT EXISTS (SELECT 1 FROM dbo.URL U WHERE U.Descripcion=A.Pagina);
    INSERT dbo.Nivel_URL(IdNivel,IdUrl,IdEstado)
    SELECT N.IdNivel,U.IdUrl,E.IdEstado FROM @Accesos A
    JOIN dbo.Nivel N ON N.Descripcion=A.Nivel
    JOIN dbo.URL U ON U.Descripcion=A.Pagina
    JOIN dbo.Estado E ON E.Descripcion='Activo'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Nivel_URL NU WHERE NU.IdNivel=N.IdNivel AND NU.IdUrl=U.IdUrl);
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
