/* Separa los modulos del medico y registra el resultado de la atencion.
   No cambia turnos ni observaciones existentes. Requiere el script 07. */
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID('dbo.HistorialAtencion','U') IS NULL
        THROW 50001,'Primero se debe ejecutar el script 07.',1;
    IF COL_LENGTH('dbo.HistorialAtencion','IdEstadoAnterior') IS NULL
        ALTER TABLE dbo.HistorialAtencion ADD IdEstadoAnterior INT NULL
            REFERENCES dbo.EstadoTurno(IdEstadoTurno);
    IF COL_LENGTH('dbo.HistorialAtencion','IdEstadoNuevo') IS NULL
        ALTER TABLE dbo.HistorialAtencion ADD IdEstadoNuevo INT NULL
            REFERENCES dbo.EstadoTurno(IdEstadoTurno);

    DECLARE @Paginas TABLE(Pagina VARCHAR(150));
    INSERT @Paginas VALUES('MiDisponibilidadMedico.aspx'),('DetalleTurnoMedico.aspx');
    INSERT dbo.URL(Descripcion)
    SELECT P.Pagina FROM @Paginas P
    WHERE NOT EXISTS(SELECT 1 FROM dbo.URL U WHERE U.Descripcion=P.Pagina);
    INSERT dbo.Nivel_URL(IdNivel,IdUrl,IdEstado)
    SELECT N.IdNivel,U.IdUrl,E.IdEstado FROM dbo.URL U
    JOIN @Paginas P ON P.Pagina=U.Descripcion
    JOIN dbo.Nivel N ON N.Descripcion='MEDICO'
    JOIN dbo.Estado E ON E.Descripcion='Activo'
    WHERE NOT EXISTS(SELECT 1 FROM dbo.Nivel_URL NU WHERE NU.IdNivel=N.IdNivel AND NU.IdUrl=U.IdUrl);
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
