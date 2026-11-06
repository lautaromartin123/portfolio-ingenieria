using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class AgendaInmediataNegocio
    {
        // =====================================================
        // PRÓXIMO TURNO
        // =====================================================

        public TurnoAgendaIA ObtenerProximoTurno(
            int idProfesional,
            DateTime desde)
        {
            ValidarProfesional(
                idProfesional);


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string sql =
                    @"
                    SELECT TOP 1
                        T.IdTurno,
                        T.FechaHora,
                        T.DuracionMinutos,
                        ET.Descripcion AS Estado

                    FROM Turno T

                    INNER JOIN EstadoTurno ET
                        ON T.IdEstadoTurno =
                           ET.IdEstadoTurno

                    WHERE
                        T.IdProfesional =
                            @IdProfesional

                        AND T.FechaHora >=
                            @Desde

                        AND ET.Descripcion <>
                            'Cancelado'

                    ORDER BY
                        T.FechaHora,
                        T.IdTurno
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        sql,
                        conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@IdProfesional",
                        idProfesional);


                    comando.Parameters.AddWithValue(
                        "@Desde",
                        desde);


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        if (!lector.Read())
                        {
                            return null;
                        }


                        return CrearTurno(
                            lector);
                    }
                }
            }
        }


        // =====================================================
        // TURNOS EN UN PERÍODO
        // =====================================================

        public List<TurnoAgendaIA> ObtenerTurnosPeriodo(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            ValidarProfesional(
                idProfesional);


            if (hasta < desde)
            {
                throw new ArgumentException(
                    "El período no es válido.");
            }


            List<TurnoAgendaIA> lista =
                new List<TurnoAgendaIA>();


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string sql =
                    @"
                    SELECT
                        T.IdTurno,
                        T.FechaHora,
                        T.DuracionMinutos,
                        ET.Descripcion AS Estado

                    FROM Turno T

                    INNER JOIN EstadoTurno ET
                        ON T.IdEstadoTurno =
                           ET.IdEstadoTurno

                    WHERE
                        T.IdProfesional =
                            @IdProfesional

                        AND T.FechaHora >=
                            @Desde

                        AND T.FechaHora <
                            @Hasta

                        AND ET.Descripcion <>
                            'Cancelado'

                    ORDER BY
                        T.FechaHora,
                        T.IdTurno
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        sql,
                        conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@IdProfesional",
                        idProfesional);


                    comando.Parameters.AddWithValue(
                        "@Desde",
                        desde);


                    comando.Parameters.AddWithValue(
                        "@Hasta",
                        hasta);


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(
                                CrearTurno(
                                    lector));
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // PRÓXIMO DÍA LABORAL SIN TURNOS
        // =====================================================

        public DateTime? ObtenerProximoDiaLaboralSinTurnos(
            int idProfesional,
            DateTime desde,
            int maximoDias)
        {
            ValidarProfesional(
                idProfesional);


            if (maximoDias <= 0)
            {
                throw new ArgumentException(
                    "El límite de días no es válido.");
            }


            List<int> diasLaborales =
                ObtenerDiasLaborales(
                    idProfesional);


            if (diasLaborales.Count == 0)
            {
                return null;
            }


            DateTime fecha =
                desde.Date;


            DateTime limite =
                desde.Date.AddDays(
                    maximoDias);


            while (fecha <= limite)
            {
                int diaSemana =
                    ConvertirDiaSemana(
                        fecha.DayOfWeek);


                if (diasLaborales.Contains(
                    diaSemana))
                {
                    DateTime desdeDia =
                        fecha.Date;


                    DateTime hastaDia =
                        fecha.Date.AddDays(1);


                    int cantidad =
                        ContarTurnos(
                            idProfesional,
                            desdeDia,
                            hastaDia);


                    if (cantidad == 0)
                    {
                        return fecha;
                    }
                }


                fecha =
                    fecha.AddDays(1);
            }


            return null;
        }


        // =====================================================
        // DÍAS LABORALES CONFIGURADOS
        // =====================================================

        private List<int> ObtenerDiasLaborales(
            int idProfesional)
        {
            List<int> lista =
                new List<int>();


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string sql =
                    @"
                    SELECT DISTINCT
                        A.DiaSemana

                    FROM Agenda A

                    INNER JOIN Estado E
                        ON A.IdEstado =
                           E.IdEstado

                    WHERE
                        A.IdProfesional =
                            @IdProfesional

                        AND E.Descripcion =
                            'Activo'

                    ORDER BY
                        A.DiaSemana
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        sql,
                        conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@IdProfesional",
                        idProfesional);


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(
                                Convert.ToInt32(
                                    lector[
                                        "DiaSemana"]));
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // CONTAR TURNOS
        // =====================================================

        private int ContarTurnos(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string sql =
                    @"
                    SELECT
                        COUNT(*)

                    FROM Turno T

                    INNER JOIN EstadoTurno ET
                        ON T.IdEstadoTurno =
                           ET.IdEstadoTurno

                    WHERE
                        T.IdProfesional =
                            @IdProfesional

                        AND T.FechaHora >=
                            @Desde

                        AND T.FechaHora <
                            @Hasta

                        AND ET.Descripcion <>
                            'Cancelado'
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        sql,
                        conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@IdProfesional",
                        idProfesional);


                    comando.Parameters.AddWithValue(
                        "@Desde",
                        desde);


                    comando.Parameters.AddWithValue(
                        "@Hasta",
                        hasta);


                    conexion.Open();


                    return Convert.ToInt32(
                        comando.ExecuteScalar());
                }
            }
        }


        // =====================================================
        // CREAR OBJETO
        // =====================================================

        private TurnoAgendaIA CrearTurno(
            SqlDataReader lector)
        {
            TurnoAgendaIA turno =
                new TurnoAgendaIA();


            turno.IdTurno =
                Convert.ToInt32(
                    lector["IdTurno"]);


            turno.FechaHora =
                Convert.ToDateTime(
                    lector["FechaHora"]);


            turno.DuracionMinutos =
                Convert.ToInt32(
                    lector[
                        "DuracionMinutos"]);


            turno.Estado =
                Convert.ToString(
                    lector["Estado"]);


            return turno;
        }


        // =====================================================
        // DÍA DE SEMANA
        // =====================================================

        private int ConvertirDiaSemana(
            DayOfWeek dia)
        {
            switch (dia)
            {
                case DayOfWeek.Monday:
                    return 1;

                case DayOfWeek.Tuesday:
                    return 2;

                case DayOfWeek.Wednesday:
                    return 3;

                case DayOfWeek.Thursday:
                    return 4;

                case DayOfWeek.Friday:
                    return 5;

                case DayOfWeek.Saturday:
                    return 6;

                case DayOfWeek.Sunday:
                    return 7;

                default:
                    return 0;
            }
        }


        // =====================================================
        // VALIDACIÓN
        // =====================================================

        private void ValidarProfesional(
            int idProfesional)
        {
            if (idProfesional <= 0)
            {
                throw new ArgumentException(
                    "El profesional no es válido.");
            }
        }
    }
}
