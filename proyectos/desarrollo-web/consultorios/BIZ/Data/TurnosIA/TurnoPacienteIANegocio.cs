using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BIZ
{
    public class TurnoPacienteIANegocio
    {
        private readonly string cadenaConexion =
            ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        public List<EspecialidadTurnoIA> ListarEspecialidadesDisponibles()
        {
            List<EspecialidadTurnoIA> lista = new List<EspecialidadTurnoIA>();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT DISTINCT
                        E.IdEspecialidad,
                        E.Nombre
                    FROM Especialidad E
                    INNER JOIN Profesional P
                        ON P.IdEspecialidad = E.IdEspecialidad
                    INNER JOIN Estado EP
                        ON EP.IdEstado = P.IdEstado
                    INNER JOIN Agenda A
                        ON A.IdProfesional = P.IdProfesional
                    INNER JOIN Estado EA
                        ON EA.IdEstado = A.IdEstado
                    WHERE EP.Descripcion = 'Activo'
                      AND EA.Descripcion = 'Activo'
                      AND A.AtencionConTurno = 1
                    ORDER BY E.Nombre";

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new EspecialidadTurnoIA
                        {
                            IdEspecialidad = Convert.ToInt32(lector["IdEspecialidad"]),
                            Nombre = lector["Nombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        public List<ProfesionalTurnoIA> ListarProfesionalesDisponibles(
            int? idEspecialidad)
        {
            List<ProfesionalTurnoIA> lista = new List<ProfesionalTurnoIA>();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT DISTINCT
                        P.IdProfesional,
                        P.Nombre,
                        P.Apellido,
                        P.IdEspecialidad,
                        E.Nombre AS Especialidad
                    FROM Profesional P
                    INNER JOIN Especialidad E
                        ON E.IdEspecialidad = P.IdEspecialidad
                    INNER JOIN Estado EP
                        ON EP.IdEstado = P.IdEstado
                    INNER JOIN Agenda A
                        ON A.IdProfesional = P.IdProfesional
                    INNER JOIN Estado EA
                        ON EA.IdEstado = A.IdEstado
                    WHERE (@IdEspecialidad IS NULL OR P.IdEspecialidad = @IdEspecialidad)
                      AND EP.Descripcion = 'Activo'
                      AND EA.Descripcion = 'Activo'
                      AND A.AtencionConTurno = 1
                    ORDER BY P.Apellido, P.Nombre";

                comando.Parameters.Add("@IdEspecialidad", SqlDbType.Int).Value =
                    (object)idEspecialidad ?? DBNull.Value;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new ProfesionalTurnoIA
                        {
                            IdProfesional = Convert.ToInt32(lector["IdProfesional"]),
                            Nombre = lector["Nombre"].ToString(),
                            Apellido = lector["Apellido"].ToString(),
                            IdEspecialidad = Convert.ToInt32(lector["IdEspecialidad"]),
                            Especialidad = lector["Especialidad"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        public List<SlotTurnoIA> ObtenerSlotsDisponibles(
            int idPaciente,
            int? idEspecialidad,
            int? idProfesional,
            DateTime desde,
            DateTime hasta,
            TimeSpan? horaDesde,
            TimeSpan? horaHasta,
            int maximo)
        {
            if (maximo < 1)
                maximo = 1;

            if (maximo > 200)
                maximo = 200;

            desde = desde.Date;
            hasta = hasta.Date.AddDays(1);

            if (hasta <= desde)
                return new List<SlotTurnoIA>();

            if ((hasta - desde).TotalDays > 62)
                hasta = desde.AddDays(62);

            List<FranjaAgendaTurnoIA> franjas =
                ObtenerFranjas(idEspecialidad, idProfesional);

            if (franjas.Count == 0)
                return new List<SlotTurnoIA>();

            List<TurnoOcupadoIA> ocupados =
                ObtenerTurnosOcupados(
                    idPaciente,
                    idEspecialidad,
                    idProfesional,
                    desde,
                    hasta);

            DateTime ahora = DateTime.Now;
            List<SlotTurnoIA> resultado = new List<SlotTurnoIA>();
            HashSet<string> claves = new HashSet<string>();

            for (DateTime fecha = desde.Date;
                 fecha < hasta.Date && resultado.Count < maximo;
                 fecha = fecha.AddDays(1))
            {
                int diaSemana = ConvertirDiaSemana(fecha.DayOfWeek);

                foreach (FranjaAgendaTurnoIA franja in franjas)
                {
                    if (franja.DiaSemana != diaSemana)
                        continue;

                    DateTime inicioFranja = fecha.Add(franja.HoraInicio);
                    DateTime finFranja = fecha.Add(franja.HoraFin);

                    for (DateTime inicio = inicioFranja;
                         inicio.AddMinutes(franja.DuracionMinutos) <= finFranja;
                         inicio = inicio.AddMinutes(franja.DuracionMinutos))
                    {
                        if (inicio <= ahora)
                            continue;

                        if (horaDesde.HasValue &&
                            inicio.TimeOfDay < horaDesde.Value)
                        {
                            continue;
                        }

                        if (horaHasta.HasValue &&
                            inicio.TimeOfDay >= horaHasta.Value)
                        {
                            continue;
                        }

                        DateTime fin = inicio.AddMinutes(franja.DuracionMinutos);

                        bool hayConflicto = ocupados.Any(t =>
                            (t.IdProfesional == franja.IdProfesional ||
                             t.IdPaciente == idPaciente)
                            && inicio < t.Fin
                            && t.Inicio < fin);

                        if (hayConflicto)
                            continue;

                        string clave =
                            franja.IdProfesional + "|" + inicio.Ticks;

                        if (!claves.Add(clave))
                            continue;

                        resultado.Add(new SlotTurnoIA
                        {
                            IdProfesional = franja.IdProfesional,
                            Profesional = franja.Profesional,
                            IdEspecialidad = franja.IdEspecialidad,
                            Especialidad = franja.Especialidad,
                            FechaHora = inicio,
                            DuracionMinutos = franja.DuracionMinutos
                        });

                        if (resultado.Count >= maximo)
                            break;
                    }

                    if (resultado.Count >= maximo)
                        break;
                }
            }

            return resultado
                .OrderBy(s => s.FechaHora)
                .ThenBy(s => s.Profesional)
                .Take(maximo)
                .ToList();
        }

        public List<DateTime> ObtenerFechasConDisponibilidad(
            int idPaciente,
            int? idEspecialidad,
            int? idProfesional,
            DateTime desde,
            DateTime hasta,
            TimeSpan? horaDesde,
            TimeSpan? horaHasta,
            int maximoFechas)
        {
            if (maximoFechas < 1)
                maximoFechas = 1;

            if (maximoFechas > 10)
                maximoFechas = 10;

            List<SlotTurnoIA> slots = ObtenerSlotsDisponibles(
                idPaciente,
                idEspecialidad,
                idProfesional,
                desde,
                hasta,
                horaDesde,
                horaHasta,
                200);

            return slots
                .Select(s => s.FechaHora.Date)
                .Distinct()
                .OrderBy(f => f)
                .Take(maximoFechas)
                .ToList();
        }

        public List<TurnoPacienteResumenIA> ListarProximosTurnosPaciente(
            int idUsuario,
            int idPaciente)
        {
            List<TurnoPacienteResumenIA> lista =
                new List<TurnoPacienteResumenIA>();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT
                        T.IdTurno,
                        T.FechaHora,
                        T.DuracionMinutos,
                        T.Motivo,
                        ET.Descripcion AS Estado,
                        P.IdProfesional,
                        P.Nombre + ' ' + P.Apellido AS Profesional,
                        E.IdEspecialidad,
                        E.Nombre AS Especialidad
                    FROM Turno T
                    INNER JOIN Paciente PA
                        ON PA.IdPaciente = T.IdPaciente
                    INNER JOIN Estado EPA
                        ON EPA.IdEstado = PA.IdEstado
                    INNER JOIN Profesional P
                        ON P.IdProfesional = T.IdProfesional
                    INNER JOIN Especialidad E
                        ON E.IdEspecialidad = P.IdEspecialidad
                    INNER JOIN EstadoTurno ET
                        ON ET.IdEstadoTurno = T.IdEstadoTurno
                    WHERE PA.IdPaciente = @IdPaciente
                      AND PA.IdUsuario = @IdUsuario
                      AND EPA.Descripcion = 'Activo'
                      AND T.FechaHora >= GETDATE()
                      AND ET.Descripcion <> 'Cancelado'
                    ORDER BY T.FechaHora";

                comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value = idPaciente;
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(MapearResumen(lector));
                    }
                }
            }

            return lista;
        }

        public ResultadoReservaTurnoIA ReservarTurno(
            int idUsuario,
            int idPaciente,
            int idProfesional,
            DateTime fechaHora,
            int duracionMinutos,
            string motivo)
        {
            ResultadoReservaTurnoIA resultado = new ResultadoReservaTurnoIA();

            if (fechaHora <= DateTime.Now)
            {
                resultado.Mensaje = "El horario elegido ya pasó.";
                return resultado;
            }

            if (duracionMinutos <= 0)
            {
                resultado.Mensaje = "La duración del turno no es válida.";
                return resultado;
            }

            if (motivo != null && motivo.Length > 300)
                motivo = motivo.Substring(0, 300);

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlTransaction transaccion =
                    conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (!PacientePerteneceAUsuario(
                            conexion,
                            transaccion,
                            idUsuario,
                            idPaciente))
                        {
                            resultado.Mensaje =
                                "No pude validar el paciente asociado a tu cuenta.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        ProfesionalTurnoIA profesional =
                            ObtenerProfesionalActivo(
                                conexion,
                                transaccion,
                                idProfesional);

                        if (profesional == null)
                        {
                            resultado.Mensaje =
                                "El profesional ya no está disponible.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        int duracionAgenda;

                        if (!HorarioPerteneceAAgenda(
                            conexion,
                            transaccion,
                            idProfesional,
                            fechaHora,
                            duracionMinutos,
                            out duracionAgenda))
                        {
                            resultado.Mensaje =
                                "Ese horario ya no pertenece a la agenda disponible del profesional.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        if (HayConflictoBloqueado(
                            conexion,
                            transaccion,
                            idPaciente,
                            idProfesional,
                            fechaHora,
                            duracionAgenda))
                        {
                            resultado.Mensaje =
                                "Ese horario acaba de ocuparse o se superpone con otro turno tuyo.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        int idPendiente = ObtenerIdEstadoTurno(
                            conexion,
                            transaccion,
                            "Pendiente");

                        if (idPendiente <= 0)
                        {
                            resultado.Mensaje =
                                "No existe el estado Pendiente en la base de datos.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        int idTurno;

                        using (SqlCommand comando = conexion.CreateCommand())
                        {
                            comando.Transaction = transaccion;
                            comando.CommandText = @"
                                INSERT INTO Turno
                                (
                                    FechaHora,
                                    DuracionMinutos,
                                    Motivo,
                                    Observaciones,
                                    IdPaciente,
                                    IdProfesional,
                                    IdEstadoTurno
                                )
                                OUTPUT INSERTED.IdTurno
                                VALUES
                                (
                                    @FechaHora,
                                    @DuracionMinutos,
                                    @Motivo,
                                    @Observaciones,
                                    @IdPaciente,
                                    @IdProfesional,
                                    @IdEstadoTurno
                                )";

                            comando.Parameters.Add("@FechaHora", SqlDbType.DateTime).Value =
                                fechaHora;
                            comando.Parameters.Add("@DuracionMinutos", SqlDbType.Int).Value =
                                duracionAgenda;
                            comando.Parameters.Add("@Motivo", SqlDbType.VarChar, 300).Value =
                                (object)motivo ?? DBNull.Value;
                            comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 500).Value =
                                "Reservado por asistente de turnos del paciente";
                            comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value =
                                idPaciente;
                            comando.Parameters.Add("@IdProfesional", SqlDbType.Int).Value =
                                idProfesional;
                            comando.Parameters.Add("@IdEstadoTurno", SqlDbType.Int).Value =
                                idPendiente;

                            idTurno = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        transaccion.Commit();

                        TurnoPacienteResumenIA turnoGuardado =
                            BuscarTurnoPaciente(idUsuario, idPaciente, idTurno);

                        if (turnoGuardado == null)
                        {
                            resultado.Mensaje =
                                "El turno fue guardado, pero no pude volver a leerlo.";
                            resultado.IdTurno = idTurno;
                            resultado.Exito = true;
                            return resultado;
                        }

                        resultado.Exito = true;
                        resultado.IdTurno = idTurno;
                        resultado.Turno = turnoGuardado;
                        resultado.Mensaje = "Turno reservado correctamente.";
                        return resultado;
                    }
                    catch
                    {
                        if (transaccion.Connection != null)
                            transaccion.Rollback();

                        throw;
                    }
                }
            }
        }

        public ResultadoOperacionTurnoIA CancelarTurnoPaciente(
            int idUsuario,
            int idPaciente,
            int idTurno)
        {
            ResultadoOperacionTurnoIA resultado =
                new ResultadoOperacionTurnoIA();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlTransaction transaccion =
                    conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (!PacientePerteneceAUsuario(
                            conexion,
                            transaccion,
                            idUsuario,
                            idPaciente))
                        {
                            resultado.Mensaje =
                                "No pude validar el paciente asociado a tu cuenta.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        int existe;

                        using (SqlCommand comando = conexion.CreateCommand())
                        {
                            comando.Transaction = transaccion;
                            comando.CommandText = @"
                                SELECT COUNT(*)
                                FROM Turno T WITH (UPDLOCK, HOLDLOCK)
                                INNER JOIN EstadoTurno ET
                                    ON ET.IdEstadoTurno = T.IdEstadoTurno
                                WHERE T.IdTurno = @IdTurno
                                  AND T.IdPaciente = @IdPaciente
                                  AND T.FechaHora >= GETDATE()
                                  AND ET.Descripcion <> 'Cancelado'";

                            comando.Parameters.Add("@IdTurno", SqlDbType.Int).Value =
                                idTurno;
                            comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value =
                                idPaciente;

                            existe = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        if (existe == 0)
                        {
                            resultado.Mensaje =
                                "Ese turno no existe, no te pertenece, ya pasó o ya está cancelado.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        int idCancelado = ObtenerIdEstadoTurno(
                            conexion,
                            transaccion,
                            "Cancelado");

                        if (idCancelado <= 0)
                        {
                            resultado.Mensaje =
                                "No existe el estado Cancelado en la base de datos.";
                            transaccion.Rollback();
                            return resultado;
                        }

                        using (SqlCommand comando = conexion.CreateCommand())
                        {
                            comando.Transaction = transaccion;
                            comando.CommandText = @"
                                UPDATE Turno
                                SET IdEstadoTurno = @IdCancelado
                                WHERE IdTurno = @IdTurno
                                  AND IdPaciente = @IdPaciente";

                            comando.Parameters.Add("@IdCancelado", SqlDbType.Int).Value =
                                idCancelado;
                            comando.Parameters.Add("@IdTurno", SqlDbType.Int).Value =
                                idTurno;
                            comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value =
                                idPaciente;

                            comando.ExecuteNonQuery();
                        }

                        transaccion.Commit();

                        resultado.Exito = true;
                        resultado.Mensaje = "Turno cancelado correctamente.";
                        return resultado;
                    }
                    catch
                    {
                        if (transaccion.Connection != null)
                            transaccion.Rollback();

                        throw;
                    }
                }
            }
        }

        public TurnoPacienteResumenIA BuscarTurnoPaciente(
            int idUsuario,
            int idPaciente,
            int idTurno)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT
                        T.IdTurno,
                        T.FechaHora,
                        T.DuracionMinutos,
                        T.Motivo,
                        ET.Descripcion AS Estado,
                        P.IdProfesional,
                        P.Nombre + ' ' + P.Apellido AS Profesional,
                        E.IdEspecialidad,
                        E.Nombre AS Especialidad
                    FROM Turno T
                    INNER JOIN Paciente PA
                        ON PA.IdPaciente = T.IdPaciente
                    INNER JOIN Profesional P
                        ON P.IdProfesional = T.IdProfesional
                    INNER JOIN Especialidad E
                        ON E.IdEspecialidad = P.IdEspecialidad
                    INNER JOIN EstadoTurno ET
                        ON ET.IdEstadoTurno = T.IdEstadoTurno
                    WHERE T.IdTurno = @IdTurno
                      AND PA.IdPaciente = @IdPaciente
                      AND PA.IdUsuario = @IdUsuario";

                comando.Parameters.Add("@IdTurno", SqlDbType.Int).Value = idTurno;
                comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value = idPaciente;
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                        return MapearResumen(lector);
                }
            }

            return null;
        }

        private List<FranjaAgendaTurnoIA> ObtenerFranjas(
            int? idEspecialidad,
            int? idProfesional)
        {
            List<FranjaAgendaTurnoIA> lista =
                new List<FranjaAgendaTurnoIA>();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT
                        A.IdProfesional,
                        P.IdEspecialidad,
                        P.Nombre + ' ' + P.Apellido AS Profesional,
                        E.Nombre AS Especialidad,
                        A.DiaSemana,
                        A.HoraInicio,
                        A.HoraFin,
                        A.DuracionMinutos
                    FROM Agenda A
                    INNER JOIN Profesional P
                        ON P.IdProfesional = A.IdProfesional
                    INNER JOIN Especialidad E
                        ON E.IdEspecialidad = P.IdEspecialidad
                    INNER JOIN Estado EA
                        ON EA.IdEstado = A.IdEstado
                    INNER JOIN Estado EP
                        ON EP.IdEstado = P.IdEstado
                    WHERE EA.Descripcion = 'Activo'
                      AND EP.Descripcion = 'Activo'
                      AND A.AtencionConTurno = 1
                      AND (@IdEspecialidad IS NULL OR P.IdEspecialidad = @IdEspecialidad)
                      AND (@IdProfesional IS NULL OR P.IdProfesional = @IdProfesional)
                    ORDER BY A.DiaSemana, A.HoraInicio, P.Apellido, P.Nombre";

                comando.Parameters.Add("@IdEspecialidad", SqlDbType.Int).Value =
                    (object)idEspecialidad ?? DBNull.Value;
                comando.Parameters.Add("@IdProfesional", SqlDbType.Int).Value =
                    (object)idProfesional ?? DBNull.Value;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new FranjaAgendaTurnoIA
                        {
                            IdProfesional = Convert.ToInt32(lector["IdProfesional"]),
                            IdEspecialidad = Convert.ToInt32(lector["IdEspecialidad"]),
                            Profesional = lector["Profesional"].ToString(),
                            Especialidad = lector["Especialidad"].ToString(),
                            DiaSemana = Convert.ToInt32(lector["DiaSemana"]),
                            HoraInicio = (TimeSpan)lector["HoraInicio"],
                            HoraFin = (TimeSpan)lector["HoraFin"],
                            DuracionMinutos = Convert.ToInt32(lector["DuracionMinutos"])
                        });
                    }
                }
            }

            return lista;
        }

        private List<TurnoOcupadoIA> ObtenerTurnosOcupados(
            int idPaciente,
            int? idEspecialidad,
            int? idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            List<TurnoOcupadoIA> lista = new List<TurnoOcupadoIA>();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT
                        T.IdProfesional,
                        T.IdPaciente,
                        T.FechaHora,
                        T.DuracionMinutos
                    FROM Turno T
                    INNER JOIN Profesional P
                        ON P.IdProfesional = T.IdProfesional
                    INNER JOIN EstadoTurno ET
                        ON ET.IdEstadoTurno = T.IdEstadoTurno
                    WHERE ET.Descripcion <> 'Cancelado'
                      AND T.FechaHora < @Hasta
                      AND DATEADD(MINUTE, T.DuracionMinutos, T.FechaHora) > @Desde
                      AND
                      (
                          T.IdPaciente = @IdPaciente
                          OR
                          (
                              (@IdProfesional IS NULL OR T.IdProfesional = @IdProfesional)
                              AND (@IdEspecialidad IS NULL OR P.IdEspecialidad = @IdEspecialidad)
                          )
                      )";

                comando.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                comando.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta;
                comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value = idPaciente;
                comando.Parameters.Add("@IdEspecialidad", SqlDbType.Int).Value =
                    (object)idEspecialidad ?? DBNull.Value;
                comando.Parameters.Add("@IdProfesional", SqlDbType.Int).Value =
                    (object)idProfesional ?? DBNull.Value;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new TurnoOcupadoIA
                        {
                            IdProfesional = Convert.ToInt32(lector["IdProfesional"]),
                            IdPaciente = Convert.ToInt32(lector["IdPaciente"]),
                            Inicio = Convert.ToDateTime(lector["FechaHora"]),
                            DuracionMinutos = Convert.ToInt32(lector["DuracionMinutos"])
                        });
                    }
                }
            }

            return lista;
        }

        private bool PacientePerteneceAUsuario(
            SqlConnection conexion,
            SqlTransaction transaccion,
            int idUsuario,
            int idPaciente)
        {
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = @"
                    SELECT COUNT(*)
                    FROM Paciente P WITH (UPDLOCK, HOLDLOCK)
                    INNER JOIN Estado E
                        ON E.IdEstado = P.IdEstado
                    WHERE P.IdPaciente = @IdPaciente
                      AND P.IdUsuario = @IdUsuario
                      AND E.Descripcion = 'Activo'";

                comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value = idPaciente;
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;

                return Convert.ToInt32(comando.ExecuteScalar()) > 0;
            }
        }

        private ProfesionalTurnoIA ObtenerProfesionalActivo(
            SqlConnection conexion,
            SqlTransaction transaccion,
            int idProfesional)
        {
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = @"
                    SELECT
                        P.IdProfesional,
                        P.Nombre,
                        P.Apellido,
                        P.IdEspecialidad,
                        ESP.Nombre AS Especialidad
                    FROM Profesional P WITH (UPDLOCK, HOLDLOCK)
                    INNER JOIN Especialidad ESP
                        ON ESP.IdEspecialidad = P.IdEspecialidad
                    INNER JOIN Estado E
                        ON E.IdEstado = P.IdEstado
                    WHERE P.IdProfesional = @IdProfesional
                      AND E.Descripcion = 'Activo'";

                comando.Parameters.Add("@IdProfesional", SqlDbType.Int).Value =
                    idProfesional;

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (!lector.Read())
                        return null;

                    return new ProfesionalTurnoIA
                    {
                        IdProfesional = Convert.ToInt32(lector["IdProfesional"]),
                        Nombre = lector["Nombre"].ToString(),
                        Apellido = lector["Apellido"].ToString(),
                        IdEspecialidad = Convert.ToInt32(lector["IdEspecialidad"]),
                        Especialidad = lector["Especialidad"].ToString()
                    };
                }
            }
        }

        private bool HorarioPerteneceAAgenda(
            SqlConnection conexion,
            SqlTransaction transaccion,
            int idProfesional,
            DateTime fechaHora,
            int duracionSolicitada,
            out int duracionAgenda)
        {
            duracionAgenda = 0;
            int diaSemana = ConvertirDiaSemana(fechaHora.DayOfWeek);
            TimeSpan horaInicio = fechaHora.TimeOfDay;

            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = @"
                    SELECT TOP 1
                        A.DuracionMinutos
                    FROM Agenda A WITH (UPDLOCK, HOLDLOCK)
                    INNER JOIN Estado E
                        ON E.IdEstado = A.IdEstado
                    WHERE A.IdProfesional = @IdProfesional
                      AND A.DiaSemana = @DiaSemana
                      AND A.AtencionConTurno = 1
                      AND E.Descripcion = 'Activo'
                      AND A.DuracionMinutos = @DuracionSolicitada
                      AND A.HoraInicio <= @HoraInicio
                      AND A.HoraFin >= @HoraFin
                      AND DATEDIFF(MINUTE, A.HoraInicio, @HoraInicio) % A.DuracionMinutos = 0
                    ORDER BY A.HoraInicio";

                comando.Parameters.Add("@IdProfesional", SqlDbType.Int).Value =
                    idProfesional;
                comando.Parameters.Add("@DiaSemana", SqlDbType.TinyInt).Value =
                    diaSemana;
                comando.Parameters.Add("@DuracionSolicitada", SqlDbType.Int).Value =
                    duracionSolicitada;
                comando.Parameters.Add("@HoraInicio", SqlDbType.Time).Value =
                    horaInicio;
                comando.Parameters.Add("@HoraFin", SqlDbType.Time).Value =
                    fechaHora.AddMinutes(duracionSolicitada).TimeOfDay;

                object valor = comando.ExecuteScalar();

                if (valor == null || valor == DBNull.Value)
                    return false;

                duracionAgenda = Convert.ToInt32(valor);
                return duracionAgenda > 0;
            }
        }

        private bool HayConflictoBloqueado(
            SqlConnection conexion,
            SqlTransaction transaccion,
            int idPaciente,
            int idProfesional,
            DateTime fechaHora,
            int duracionMinutos)
        {
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = @"
                    SELECT COUNT(*)
                    FROM Turno T WITH (UPDLOCK, HOLDLOCK)
                    INNER JOIN EstadoTurno ET
                        ON ET.IdEstadoTurno = T.IdEstadoTurno
                    WHERE ET.Descripcion <> 'Cancelado'
                      AND (T.IdProfesional = @IdProfesional OR T.IdPaciente = @IdPaciente)
                      AND @Inicio < DATEADD(MINUTE, T.DuracionMinutos, T.FechaHora)
                      AND T.FechaHora < @Fin";

                comando.Parameters.Add("@IdProfesional", SqlDbType.Int).Value =
                    idProfesional;
                comando.Parameters.Add("@IdPaciente", SqlDbType.Int).Value =
                    idPaciente;
                comando.Parameters.Add("@Inicio", SqlDbType.DateTime).Value =
                    fechaHora;
                comando.Parameters.Add("@Fin", SqlDbType.DateTime).Value =
                    fechaHora.AddMinutes(duracionMinutos);

                return Convert.ToInt32(comando.ExecuteScalar()) > 0;
            }
        }

        private int ObtenerIdEstadoTurno(
            SqlConnection conexion,
            SqlTransaction transaccion,
            string descripcion)
        {
            using (SqlCommand comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = @"
                    SELECT TOP 1 IdEstadoTurno
                    FROM EstadoTurno
                    WHERE Descripcion = @Descripcion";

                comando.Parameters.Add("@Descripcion", SqlDbType.VarChar, 50).Value =
                    descripcion;

                object valor = comando.ExecuteScalar();

                if (valor == null || valor == DBNull.Value)
                    return 0;

                return Convert.ToInt32(valor);
            }
        }

        private TurnoPacienteResumenIA MapearResumen(SqlDataReader lector)
        {
            return new TurnoPacienteResumenIA
            {
                IdTurno = Convert.ToInt32(lector["IdTurno"]),
                FechaHora = Convert.ToDateTime(lector["FechaHora"]),
                DuracionMinutos = Convert.ToInt32(lector["DuracionMinutos"]),
                Motivo = lector["Motivo"] == DBNull.Value
                    ? null
                    : lector["Motivo"].ToString(),
                Estado = lector["Estado"].ToString(),
                IdProfesional = Convert.ToInt32(lector["IdProfesional"]),
                Profesional = lector["Profesional"].ToString(),
                IdEspecialidad = Convert.ToInt32(lector["IdEspecialidad"]),
                Especialidad = lector["Especialidad"].ToString()
            };
        }

        private int ConvertirDiaSemana(DayOfWeek dia)
        {
            switch (dia)
            {
                case DayOfWeek.Monday: return 1;
                case DayOfWeek.Tuesday: return 2;
                case DayOfWeek.Wednesday: return 3;
                case DayOfWeek.Thursday: return 4;
                case DayOfWeek.Friday: return 5;
                case DayOfWeek.Saturday: return 6;
                case DayOfWeek.Sunday: return 7;
                default: return 0;
            }
        }
    }
}
