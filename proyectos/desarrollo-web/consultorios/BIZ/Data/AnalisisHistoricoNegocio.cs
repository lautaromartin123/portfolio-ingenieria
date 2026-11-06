using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class AnalisisHistoricoNegocio
    {
        // =====================================================
        // REGISTRO INTERNO
        // =====================================================

        private class TurnoHistoricoInterno
        {
            public DateTime FechaHora
            {
                get;
                set;
            }


            public string Estado
            {
                get;
                set;
            }
        }


        // =====================================================
        // RESUMEN HISTÓRICO
        // =====================================================

        public ResumenHistoricoIA ObtenerResumen(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            ValidarPeriodo(
                idProfesional,
                desde,
                hasta);


            desde =
                desde.Date;


            hasta =
                hasta.Date;


            ResumenHistoricoIA resultado =
                new ResumenHistoricoIA();


            resultado.Desde =
                desde;


            resultado.Hasta =
                hasta;


            // =================================================
            // PREPARAR TODOS LOS MESES DEL PERÍODO
            // INCLUSO SI ALGUNO TIENE 0 TURNOS
            // =================================================

            CrearMesesPeriodo(
                resultado,
                desde,
                hasta);


            // =================================================
            // PREPARAR LOS 7 DÍAS DE LA SEMANA
            // =================================================

            CrearDiasSemana(
                resultado);


            // =================================================
            // OBTENER TURNOS
            // =================================================

            List<TurnoHistoricoInterno> turnos =
                ObtenerTurnos(
                    idProfesional,
                    desde,
                    hasta);


            // =================================================
            // PROCESAR
            // =================================================

            foreach (
                TurnoHistoricoInterno turno
                    in turnos)
            {
                resultado.TotalTurnos++;


                bool cancelado =
                    string.Equals(
                        turno.Estado,
                        "Cancelado",
                        StringComparison
                            .OrdinalIgnoreCase);


                bool atendido =
                    string.Equals(
                        turno.Estado,
                        "Atendido",
                        StringComparison
                            .OrdinalIgnoreCase);


                if (cancelado)
                {
                    resultado.Cancelados++;
                }
                else
                {
                    resultado
                        .TurnosNoCancelados++;
                }


                if (atendido)
                {
                    resultado.Atendidos++;
                }


                if (!cancelado &&
                    !atendido)
                {
                    resultado.OtrosEstados++;
                }


                // =============================================
                // MES
                // =============================================

                MesActividadHistoricaIA mes =
                    BuscarMes(
                        resultado.Meses,
                        turno.FechaHora.Year,
                        turno.FechaHora.Month);


                if (mes != null)
                {
                    mes.TotalTurnos++;


                    if (cancelado)
                    {
                        mes.Cancelados++;
                    }
                    else
                    {
                        mes.TurnosNoCancelados++;
                    }


                    if (atendido)
                    {
                        mes.Atendidos++;
                    }
                }


                // =============================================
                // DÍA DE SEMANA
                // =============================================

                int numeroDia =
                    ConvertirDiaSemana(
                        turno.FechaHora.DayOfWeek);


                DiaSemanaHistoricoIA dia =
                    BuscarDia(
                        resultado.DiasSemana,
                        numeroDia);


                if (dia != null)
                {
                    dia.TotalTurnos++;


                    if (cancelado)
                    {
                        dia.Cancelados++;
                    }
                    else
                    {
                        dia.TurnosNoCancelados++;
                    }


                    if (atendido)
                    {
                        dia.Atendidos++;
                    }
                }
            }


            CalcularIndicadores(
                resultado);


            return resultado;
        }


        // =====================================================
        // OBTENER ÚLTIMOS N MESES
        // =====================================================

        public ResumenHistoricoIA ObtenerUltimosMeses(
            int idProfesional,
            int cantidadMeses)
        {
            if (cantidadMeses <= 0)
            {
                throw new ArgumentException(
                    "La cantidad de meses debe ser mayor a 0.");
            }


            if (cantidadMeses > 60)
            {
                throw new ArgumentException(
                    "El máximo permitido es de 60 meses.");
            }


            DateTime hoy =
                DateTime.Today;


            DateTime primerDiaMesActual =
                new DateTime(
                    hoy.Year,
                    hoy.Month,
                    1);


            DateTime desde =
                primerDiaMesActual
                    .AddMonths(
                        -(cantidadMeses - 1));


            DateTime hasta =
                primerDiaMesActual
                    .AddMonths(1)
                    .AddDays(-1);


            return ObtenerResumen(
                idProfesional,
                desde,
                hasta);
        }


        // =====================================================
        // TURNOS DESDE SQL
        // =====================================================

        private List<TurnoHistoricoInterno> ObtenerTurnos(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            List<TurnoHistoricoInterno> lista =
                new List<TurnoHistoricoInterno>();


            DateTime hastaExclusivo =
                hasta.Date.AddDays(1);


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string sql =
                    @"
                    SELECT
                        T.FechaHora,
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
                            @HastaExclusivo

                    ORDER BY
                        T.FechaHora
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
                        desde.Date);


                    comando.Parameters.AddWithValue(
                        "@HastaExclusivo",
                        hastaExclusivo);


                    conexion.Open();


                    using (SqlDataReader lector =
                        comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            TurnoHistoricoInterno turno =
                                new TurnoHistoricoInterno();


                            turno.FechaHora =
                                Convert.ToDateTime(
                                    lector[
                                        "FechaHora"]);


                            turno.Estado =
                                Convert.ToString(
                                    lector[
                                        "Estado"]);


                            lista.Add(
                                turno);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // CREAR MESES DEL PERÍODO
        // =====================================================

        private void CrearMesesPeriodo(
            ResumenHistoricoIA resultado,
            DateTime desde,
            DateTime hasta)
        {
            DateTime mesActual =
                new DateTime(
                    desde.Year,
                    desde.Month,
                    1);


            DateTime ultimoMes =
                new DateTime(
                    hasta.Year,
                    hasta.Month,
                    1);


            while (mesActual <=
                   ultimoMes)
            {
                MesActividadHistoricaIA mes =
                    new MesActividadHistoricaIA();


                mes.Anio =
                    mesActual.Year;


                mes.Mes =
                    mesActual.Month;


                resultado.Meses.Add(
                    mes);


                mesActual =
                    mesActual.AddMonths(1);
            }
        }


        // =====================================================
        // CREAR DÍAS DE LA SEMANA
        // =====================================================

        private void CrearDiasSemana(
            ResumenHistoricoIA resultado)
        {
            for (int i = 1;
                 i <= 7;
                 i++)
            {
                DiaSemanaHistoricoIA dia =
                    new DiaSemanaHistoricoIA();


                dia.DiaSemana =
                    i;


                resultado.DiasSemana.Add(
                    dia);
            }
        }


        // =====================================================
        // BUSCAR MES
        // =====================================================

        private MesActividadHistoricaIA BuscarMes(
            List<MesActividadHistoricaIA> meses,
            int anio,
            int mes)
        {
            foreach (
                MesActividadHistoricaIA item
                    in meses)
            {
                if (item.Anio == anio &&
                    item.Mes == mes)
                {
                    return item;
                }
            }


            return null;
        }


        // =====================================================
        // BUSCAR DÍA
        // =====================================================

        private DiaSemanaHistoricoIA BuscarDia(
            List<DiaSemanaHistoricoIA> dias,
            int diaSemana)
        {
            foreach (
                DiaSemanaHistoricoIA item
                    in dias)
            {
                if (item.DiaSemana ==
                    diaSemana)
                {
                    return item;
                }
            }


            return null;
        }


        // =====================================================
        // INDICADORES
        // =====================================================

        private void CalcularIndicadores(
            ResumenHistoricoIA resultado)
        {
            // =================================================
            // TASAS
            // =================================================

            if (resultado.TotalTurnos > 0)
            {
                resultado.TasaAtencion =
                    Math.Round(
                        (
                            (decimal)
                            resultado.Atendidos
                            /
                            resultado.TotalTurnos
                        )
                        *
                        100,
                        1);


                resultado.TasaCancelacion =
                    Math.Round(
                        (
                            (decimal)
                            resultado.Cancelados
                            /
                            resultado.TotalTurnos
                        )
                        *
                        100,
                        1);
            }
            else
            {
                resultado.TasaAtencion =
                    0;


                resultado.TasaCancelacion =
                    0;
            }


            if (resultado.TurnosNoCancelados > 0)
            {
                resultado
                    .TasaAtencionSobreNoCancelados =
                    Math.Round(
                        (
                            (decimal)
                            resultado.Atendidos
                            /
                            resultado
                                .TurnosNoCancelados
                        )
                        *
                        100,
                        1);
            }
            else
            {
                resultado
                    .TasaAtencionSobreNoCancelados =
                    0;
            }


            // =================================================
            // PROMEDIOS
            // =================================================

            if (resultado.Meses.Count > 0)
            {
                resultado.PromedioTurnosPorMes =
                    Math.Round(
                        (
                            (decimal)
                            resultado.TotalTurnos
                            /
                            resultado.Meses.Count
                        ),
                        1);


                resultado.PromedioAtendidosPorMes =
                    Math.Round(
                        (
                            (decimal)
                            resultado.Atendidos
                            /
                            resultado.Meses.Count
                        ),
                        1);
            }


            // =================================================
            // MESES DESTACADOS
            // =================================================

            foreach (
                MesActividadHistoricaIA mes
                    in resultado.Meses)
            {
                if (resultado.MesMasActivo == null
                    ||
                    mes.TurnosNoCancelados >
                    resultado
                        .MesMasActivo
                        .TurnosNoCancelados)
                {
                    resultado.MesMasActivo =
                        mes;
                }


                if (resultado.MesConMasAtendidos == null
                    ||
                    mes.Atendidos >
                    resultado
                        .MesConMasAtendidos
                        .Atendidos)
                {
                    resultado.MesConMasAtendidos =
                        mes;
                }
            }


            // =================================================
            // DÍAS DESTACADOS
            // =================================================

            foreach (
                DiaSemanaHistoricoIA dia
                    in resultado.DiasSemana)
            {
                if (resultado.DiaSemanaMasActivo == null
                    ||
                    dia.TurnosNoCancelados >
                    resultado
                        .DiaSemanaMasActivo
                        .TurnosNoCancelados)
                {
                    resultado.DiaSemanaMasActivo =
                        dia;
                }


                if (resultado
                        .DiaSemanaConMasAtendidos == null
                    ||
                    dia.Atendidos >
                    resultado
                        .DiaSemanaConMasAtendidos
                        .Atendidos)
                {
                    resultado
                        .DiaSemanaConMasAtendidos =
                        dia;
                }
            }
        }


        // =====================================================
        // CONVERTIR DÍA
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

        private void ValidarPeriodo(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            if (idProfesional <= 0)
            {
                throw new ArgumentException(
                    "El profesional no es válido.");
            }


            if (hasta.Date <
                desde.Date)
            {
                throw new ArgumentException(
                    "El período no es válido.");
            }


            int dias =
                (
                    hasta.Date
                    -
                    desde.Date
                ).Days
                +
                1;


            // Hasta 10 años de análisis histórico.

            if (dias > 3653)
            {
                throw new ArgumentException(
                    "El período histórico máximo "
                    +
                    "permitido es de 10 años.");
            }
        }
    }
}
