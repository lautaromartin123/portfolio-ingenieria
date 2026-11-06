/* Solicitudes de modificación de médicos.
   Reutiliza SolicitudModificacion y EstadoSolicitud del circuito de pacientes.
   Se puede ejecutar más de una vez sin duplicar tablas ni permisos. */

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.SolicitudModificacion', 'U') IS NULL
        THROW 50001, 'Primero debe existir la tabla SolicitudModificacion.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.EstadoSolicitud WHERE IdEstadoSolicitud = 1 AND Descripcion = 'Pendiente')
       OR NOT EXISTS (SELECT 1 FROM dbo.EstadoSolicitud WHERE IdEstadoSolicitud = 2 AND Descripcion = 'Aprobada')
       OR NOT EXISTS (SELECT 1 FROM dbo.EstadoSolicitud WHERE IdEstadoSolicitud = 3 AND Descripcion = 'Rechazada')
        THROW 50002, 'Los estados de solicitud no coinciden con el circuito existente.', 1;

    IF OBJECT_ID('dbo.SolicitudModificacionMedico', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SolicitudModificacionMedico
        (
            IdSolicitud INT NOT NULL
                CONSTRAINT PK_SolicitudModificacionMedico PRIMARY KEY,
            Nombre VARCHAR(100) NOT NULL,
            Apellido VARCHAR(100) NOT NULL,
            IdEspecialidad INT NOT NULL,
            CONSTRAINT FK_SolicitudModificacionMedico_Solicitud
                FOREIGN KEY (IdSolicitud)
                REFERENCES dbo.SolicitudModificacion (IdSolicitud),
            CONSTRAINT FK_SolicitudModificacionMedico_Especialidad
                FOREIGN KEY (IdEspecialidad)
                REFERENCES dbo.Especialidad (IdEspecialidad)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.SolicitudModificacion')
          AND name = 'UX_SolicitudModificacion_ProfesionalPendiente'
    )
    BEGIN
        CREATE UNIQUE INDEX UX_SolicitudModificacion_ProfesionalPendiente
            ON dbo.SolicitudModificacion (IdProfesional)
            WHERE IdEstadoSolicitud = 1 AND IdProfesional IS NOT NULL;
    END;

    DECLARE @IdNivelMedico INT =
        (SELECT IdNivel FROM dbo.Nivel WHERE Descripcion = 'MEDICO');
    DECLARE @IdEstadoActivo INT =
        (SELECT IdEstado FROM dbo.Estado WHERE Descripcion = 'Activo');

    IF @IdNivelMedico IS NULL OR @IdEstadoActivo IS NULL
        THROW 50003, 'Deben existir el nivel MEDICO y el estado Activo.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.URL WHERE Descripcion = 'MisDatosMedico.aspx')
        INSERT INTO dbo.URL (Descripcion) VALUES ('MisDatosMedico.aspx');

    INSERT INTO dbo.Nivel_URL (IdNivel, IdUrl, IdEstado)
    SELECT @IdNivelMedico, U.IdUrl, @IdEstadoActivo
    FROM dbo.URL U
    WHERE U.Descripcion = 'MisDatosMedico.aspx'
      AND NOT EXISTS
      (
          SELECT 1 FROM dbo.Nivel_URL NU
          WHERE NU.IdNivel = @IdNivelMedico AND NU.IdUrl = U.IdUrl
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
