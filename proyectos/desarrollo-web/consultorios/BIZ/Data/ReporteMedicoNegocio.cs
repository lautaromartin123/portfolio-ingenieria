using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BIZ
{
    public class ReporteMedicoNegocio
    {
        private readonly string cadenaConexion =
            ConfigurationManager
                .ConnectionStrings["Conexion"]
                .ConnectionString;


        // =====================================================
        // DÍA CON MÁS PACIENTES ATENDIDOS
        // =====================================================

        public DiaMasAtendidoReporte ObtenerDiaConMasAtendidos(
            int idProfesional)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT TOP 1
                        CAST(T.FechaHora AS DATE) AS Fecha,
                        COUNT(*) AS Cantidad

                    FROM Turno T

                    INNER JOIN EstadoTurno ET
                        ON T.IdEstadoTurno =
                           ET.IdEstadoTurno

                    WHERE T.IdProfesional =
                          @IdProfesional

                      AND ET.Descripcion =
                          'Atendido'

                    GROUP BY
                        CAST(T.FechaHora AS DATE)

                    ORDER BY
                        COUNT(*) DESC,
                        CAST(T.FechaHora AS DATE) DESC";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdProfesional",
                        SqlDbType.Int
                    ).Value = idProfesional;


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        if (!lector.Read())
                        {
                            return null;
                        }


                        DiaMasAtendidoReporte resultado =
                            new DiaMasAtendidoReporte();


                        resultado.Fecha =
                            Convert.ToDateTime(
                                lector["Fecha"]);


                        resultado.CantidadAtendidos =
                            Convert.ToInt32(
                                lector["Cantidad"]);


                        return resultado;
                    }
                }
            }
        }


        // =====================================================
        // DÍAS Y HORARIOS DE TRABAJO
        // =====================================================

        public List<HorarioTrabajoReporte> ListarHorariosTrabajo(
            int idProfesional)
        {
            List<HorarioTrabajoReporte> lista =
                new List<HorarioTrabajoReporte>();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        A.DiaSemana,
                        A.HoraInicio,
                        A.HoraFin

                    FROM Agenda A

                    INNER JOIN Estado E
                        ON A.IdEstado =
                           E.IdEstado

                    WHERE A.IdProfesional =
                          @IdProfesional

                      AND E.Descripcion =
                          'Activo'

                    ORDER BY
                        A.DiaSemana,
                        A.HoraInicio";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdProfesional",
                        SqlDbType.Int
                    ).Value = idProfesional;


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            HorarioTrabajoReporte item =
                                new HorarioTrabajoReporte();


                            item.DiaSemana =
                                Convert.ToInt32(
                                    lector["DiaSemana"]);


                            item.HoraInicio =
                                (TimeSpan)
                                lector["HoraInicio"];


                            item.HoraFin =
                                (TimeSpan)
                                lector["HoraFin"];


                            lista.Add(item);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // AGENDA DE LA PRÓXIMA SEMANA
        // =====================================================

        public List<AgendaDiaReporte> ListarAgendaSemana(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            List<AgendaDiaReporte> lista =
                new List<AgendaDiaReporte>();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        CAST(T.FechaHora AS DATE) AS Fecha,
                        COUNT(*) AS Cantidad,
                        MIN(T.FechaHora) AS PrimerTurno,
                        MAX(T.FechaHora) AS UltimoTurno

                    FROM Turno T

                    INNER JOIN EstadoTurno ET
                        ON T.IdEstadoTurno =
                           ET.IdEstadoTurno

                    WHERE T.IdProfesional =
                          @IdProfesional

                      AND T.FechaHora >= @Desde

                      AND T.FechaHora < @Hasta

                      AND ET.Descripcion <>
                          'Cancelado'

                    GROUP BY
                        CAST(T.FechaHora AS DATE)

                    ORDER BY
                        CAST(T.FechaHora AS DATE)";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdProfesional",
                        SqlDbType.Int
                    ).Value = idProfesional;


                    comando.Parameters.Add(
                        "@Desde",
                        SqlDbType.DateTime
                    ).Value = desde;


                    comando.Parameters.Add(
                        "@Hasta",
                        SqlDbType.DateTime
                    ).Value = hasta;


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            AgendaDiaReporte item =
                                new AgendaDiaReporte();


                            item.Fecha =
                                Convert.ToDateTime(
                                    lector["Fecha"]);


                            item.CantidadTurnos =
                                Convert.ToInt32(
                                    lector["Cantidad"]);


                            item.PrimerTurno =
                                Convert.ToDateTime(
                                    lector["PrimerTurno"]);


                            item.UltimoTurno =
                                Convert.ToDateTime(
                                    lector["UltimoTurno"]);


                            lista.Add(item);
                        }
                    }
                }
            }


            return lista;
        }
    }
}
