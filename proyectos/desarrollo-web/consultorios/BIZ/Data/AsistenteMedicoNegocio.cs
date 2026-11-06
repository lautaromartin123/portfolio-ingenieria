using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class AsistenteMedicoNegocio
    {
        // =====================================================
        // CLASES INTERNAS
        // =====================================================

        private class TurnoOperativoInterno
        {
            public int IdTurno
            {
                get;
                set;
            }


            public DateTime FechaHora
            {
                get;
                set;
            }


            public int DuracionMinutos
            {
                get;
                set;
            }


            public DateTime FechaFin
            {
                get
                {
                    return FechaHora.AddMinutes(
                        DuracionMinutos);
                }
            }
        }


        private class HorarioAgendaInterno
        {
            public int DiaSemana
            {
                get;
                set;
            }


            public TimeSpan HoraInicio
            {
                get;
                set;
            }


            public TimeSpan HoraFin
            {
                get;
                set;
            }
        }


        // =====================================================
        // RESUMEN COMPLETO DE PERÍODO
        // =====================================================

        public ResumenPeriodoIA ObtenerResumenPeriodo(
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


            ResumenPeriodoIA resumen =
                new ResumenPeriodoIA();


            resumen.Desde =
                desde;


            resumen.Hasta =
                hasta;


            resumen.DiasPeriodo =
                (hasta - desde).Days + 1;


            resumen.Dias =
                ListarCargaPorDia(
                    idProfesional,
                    desde,
                    hasta);


            resumen.Estados =
                ListarEstadosPeriodo(
                    idProfesional,
                    desde,
                    hasta);


            CalcularResumen(
                resumen);


            return resumen;
        }


        // =====================================================
        // ANALIZAR ANOMALÍAS DE AGENDA
        // =====================================================

        public AnalisisAgendaIA AnalizarAgenda(
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


            AnalisisAgendaIA resultado =
                new AnalisisAgendaIA();


            resultado.Desde =
                desde;


            resultado.Hasta =
                hasta;


            List<TurnoOperativoInterno> turnos =
                ObtenerTurnosOperativos(
                    idProfesional,
                    desde,
                    hasta);


            List<HorarioAgendaInterno> horarios =
                ObtenerHorariosActivos(
                    idProfesional);


            resultado.TurnosAnalizados =
                turnos.Count;


            // =================================================
            // DÍAS NO LABORABLES Y FUERA DE HORARIO
            // =================================================

            foreach (
                TurnoOperativoInterno turno
                    in turnos)
            {
                int diaSemana =
                    ConvertirDiaSemana(
                        turno.FechaHora.DayOfWeek);


                List<HorarioAgendaInterno> horariosDia =
                    new List<HorarioAgendaInterno>();


                foreach (
                    HorarioAgendaInterno horario
                        in horarios)
                {
                    if (horario.DiaSemana ==
                        diaSemana)
                    {
                        horariosDia.Add(
                            horario);
                    }
                }


                // =============================================
                // EL MÉDICO NO TRABAJA ESE DÍA
                // =============================================

                if (horariosDia.Count == 0)
                {
                    resultado.TurnosEnDiaNoLaborable++;


                    AlertaAgendaIA alerta =
                        new AlertaAgendaIA();


                    alerta.Tipo =
                        "DIA_NO_LABORABLE";


                    alerta.FechaHora =
                        turno.FechaHora;


                    alerta.Descripcion =
                        "Existe un turno el "
                        +
                        turno.FechaHora.ToString(
                            "dd/MM/yyyy HH:mm")
                        +
                        " en un día donde el profesional "
                        +
                        "no tiene una agenda activa.";


                    resultado.Alertas.Add(
                        alerta);


                    continue;
                }


                // =============================================
                // VERIFICAR HORARIO
                // =============================================

                bool dentroDeHorario =
                    false;


                TimeSpan horaInicioTurno =
                    turno.FechaHora.TimeOfDay;


                TimeSpan horaFinTurno =
                    turno.FechaFin.TimeOfDay;


                foreach (
                    HorarioAgendaInterno horario
                        in horariosDia)
                {
                    if (horaInicioTurno >=
                            horario.HoraInicio
                        &&
                        horaFinTurno <=
                            horario.HoraFin)
                    {
                        dentroDeHorario =
                            true;

                        break;
                    }
                }


                if (!dentroDeHorario)
                {
                    resultado.TurnosFueraHorario++;


                    AlertaAgendaIA alerta =
                        new AlertaAgendaIA();


                    alerta.Tipo =
                        "FUERA_DE_HORARIO";


                    alerta.FechaHora =
                        turno.FechaHora;


                    alerta.Descripcion =
                        "El turno del "
                        +
                        turno.FechaHora.ToString(
                            "dd/MM/yyyy HH:mm")
                        +
                        " finaliza a las "
                        +
                        turno.FechaFin.ToString(
                            "HH:mm")
                        +
                        " y no entra completamente "
                        +
                        "dentro del horario laboral "
                        +
                        "configurado.";


                    resultado.Alertas.Add(
                        alerta);
                }
            }


            // =================================================
            // SUPERPOSICIONES
            // =================================================

            for (int i = 0;
                 i < turnos.Count;
                 i++)
            {
                TurnoOperativoInterno primero =
                    turnos[i];


                for (int j = i + 1;
                     j < turnos.Count;
                     j++)
                {
                    TurnoOperativoInterno segundo =
                        turnos[j];


                    bool seSuperponen =
                        primero.FechaHora <
                            segundo.FechaFin
                        &&
                        segundo.FechaHora <
                            primero.FechaFin;


                    if (seSuperponen)
                    {
                        resultado.Superposiciones++;


                        AlertaAgendaIA alerta =
                            new AlertaAgendaIA();


                        alerta.Tipo =
                            "SUPERPOSICION";


                        alerta.FechaHora =
                            primero.FechaHora <
                            segundo.FechaHora
                                ? primero.FechaHora
                                : segundo.FechaHora;


                        alerta.Descripcion =
                            "Se detectó una superposición "
                            +
                            "entre los turnos "
                            +
                            primero.IdTurno
                            +
                            " y "
                            +
                            segundo.IdTurno
                            +
                            ". Horarios: "
                            +
                            primero.FechaHora.ToString(
                                "dd/MM/yyyy HH:mm")
                            +
                            "-"
                            +
                            primero.FechaFin.ToString(
                                "HH:mm")
                            +
                            " y "
                            +
                            segundo.FechaHora.ToString(
                                "dd/MM/yyyy HH:mm")
                            +
                            "-"
                            +
                            segundo.FechaFin.ToString(
                                "HH:mm")
                            +
                            ".";


                        resultado.Alertas.Add(
                            alerta);
                    }
                }
            }


            return resultado;
        }


        // =====================================================
        // CARGA POR DÍA
        // =====================================================

        public List<DiaAgendaIA> ListarCargaPorDia(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            ValidarPeriodo(
                idProfesional,
                desde,
                hasta);


            List<DiaAgendaIA> lista =
                new List<DiaAgendaIA>();


            DateTime hastaExclusivo =
                hasta.Date.AddDays(1);


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string consulta =
                    @"
                    SELECT
                        CAST(T.FechaHora AS DATE) AS Fecha,
                        COUNT(*) AS CantidadTurnos,
                        MIN(T.FechaHora) AS PrimerTurno,
                        MAX(T.FechaHora) AS UltimoTurno

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

                        AND ET.Descripcion <>
                            'Cancelado'

                    GROUP BY
                        CAST(T.FechaHora AS DATE)

                    ORDER BY
                        CAST(T.FechaHora AS DATE)
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        consulta,
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
                            DiaAgendaIA dia =
                                new DiaAgendaIA();


                            dia.Fecha =
                                Convert.ToDateTime(
                                    lector["Fecha"]);


                            dia.CantidadTurnos =
                                Convert.ToInt32(
                                    lector[
                                        "CantidadTurnos"]);


                            dia.PrimerTurno =
                                Convert.ToDateTime(
                                    lector[
                                        "PrimerTurno"]);


                            dia.UltimoTurno =
                                Convert.ToDateTime(
                                    lector[
                                        "UltimoTurno"]);


                            lista.Add(
                                dia);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // ESTADOS DE TURNOS
        // =====================================================

        public List<EstadoTurnoCantidadIA>
            ListarEstadosPeriodo(
                int idProfesional,
                DateTime desde,
                DateTime hasta)
        {
            ValidarPeriodo(
                idProfesional,
                desde,
                hasta);


            List<EstadoTurnoCantidadIA> lista =
                new List<EstadoTurnoCantidadIA>();


            DateTime hastaExclusivo =
                hasta.Date.AddDays(1);


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string consulta =
                    @"
                    SELECT
                        ET.Descripcion AS Estado,
                        COUNT(*) AS Cantidad

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

                    GROUP BY
                        ET.Descripcion

                    ORDER BY
                        ET.Descripcion
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        consulta,
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
                            EstadoTurnoCantidadIA item =
                                new EstadoTurnoCantidadIA();


                            item.Estado =
                                Convert.ToString(
                                    lector["Estado"]);


                            item.Cantidad =
                                Convert.ToInt32(
                                    lector["Cantidad"]);


                            lista.Add(
                                item);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // OBTENER TURNOS PARA ANÁLISIS
        // =====================================================

        private List<TurnoOperativoInterno>
            ObtenerTurnosOperativos(
                int idProfesional,
                DateTime desde,
                DateTime hasta)
        {
            List<TurnoOperativoInterno> lista =
                new List<TurnoOperativoInterno>();


            DateTime hastaExclusivo =
                hasta.Date.AddDays(1);


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string consulta =
                    @"
                    SELECT
                        T.IdTurno,
                        T.FechaHora,
                        T.DuracionMinutos

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

                        AND ET.Descripcion <>
                            'Cancelado'

                    ORDER BY
                        T.FechaHora,
                        T.IdTurno
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        consulta,
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
                            TurnoOperativoInterno turno =
                                new TurnoOperativoInterno();


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


                            lista.Add(
                                turno);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // HORARIOS ACTIVOS DEL PROFESIONAL
        // =====================================================

        private List<HorarioAgendaInterno>
            ObtenerHorariosActivos(
                int idProfesional)
        {
            List<HorarioAgendaInterno> lista =
                new List<HorarioAgendaInterno>();


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string consulta =
                    @"
                    SELECT
                        A.DiaSemana,
                        A.HoraInicio,
                        A.HoraFin

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
                        A.DiaSemana,
                        A.HoraInicio
                    ";


                using (SqlCommand comando =
                    new SqlCommand(
                        consulta,
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
                            HorarioAgendaInterno horario =
                                new HorarioAgendaInterno();


                            horario.DiaSemana =
                                Convert.ToInt32(
                                    lector[
                                        "DiaSemana"]);


                            horario.HoraInicio =
                                (TimeSpan)
                                lector["HoraInicio"];


                            horario.HoraFin =
                                (TimeSpan)
                                lector["HoraFin"];


                            lista.Add(
                                horario);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // CONVERTIR DayOfWeek
        //
        // BASE:
        // 1 = lunes
        // 2 = martes
        // ...
        // 7 = domingo
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
        // CÁLCULOS DEL RESUMEN
        // =====================================================

        private void CalcularResumen(
            ResumenPeriodoIA resumen)
        {
            int total =
                0;


            int cancelados =
                0;


            int atendidos =
                0;


            foreach (
                EstadoTurnoCantidadIA estado
                    in resumen.Estados)
            {
                total +=
                    estado.Cantidad;


                if (string.Equals(
                    estado.Estado,
                    "Cancelado",
                    StringComparison
                        .OrdinalIgnoreCase))
                {
                    cancelados +=
                        estado.Cantidad;
                }


                if (string.Equals(
                    estado.Estado,
                    "Atendido",
                    StringComparison
                        .OrdinalIgnoreCase))
                {
                    atendidos +=
                        estado.Cantidad;
                }
            }


            resumen.TotalTurnos =
                total;


            resumen.Cancelados =
                cancelados;


            resumen.Atendidos =
                atendidos;


            resumen.TurnosNoCancelados =
                total - cancelados;


            resumen.DiasConTurnos =
                resumen.Dias.Count;


            if (resumen.Dias.Count > 0)
            {
                decimal promedio =
                    (decimal)resumen.TurnosNoCancelados
                    /
                    resumen.Dias.Count;


                resumen.PromedioTurnosPorDiaConActividad =
                    Math.Round(
                        promedio,
                        2);


                DiaAgendaIA masCargado =
                    resumen.Dias[0];


                DiaAgendaIA menosCargado =
                    resumen.Dias[0];


                DateTime primerTurno =
                    resumen.Dias[0]
                        .PrimerTurno;


                DateTime ultimoTurno =
                    resumen.Dias[0]
                        .UltimoTurno;


                foreach (
                    DiaAgendaIA dia
                        in resumen.Dias)
                {
                    if (dia.CantidadTurnos >
                        masCargado.CantidadTurnos)
                    {
                        masCargado =
                            dia;
                    }


                    if (dia.CantidadTurnos <
                        menosCargado.CantidadTurnos)
                    {
                        menosCargado =
                            dia;
                    }


                    if (dia.PrimerTurno <
                        primerTurno)
                    {
                        primerTurno =
                            dia.PrimerTurno;
                    }


                    if (dia.UltimoTurno >
                        ultimoTurno)
                    {
                        ultimoTurno =
                            dia.UltimoTurno;
                    }
                }


                resumen.DiaMasCargado =
                    masCargado.Fecha;


                resumen.CantidadDiaMasCargado =
                    masCargado.CantidadTurnos;


                resumen.DiaMenosCargadoConTurnos =
                    menosCargado.Fecha;


                resumen
                    .CantidadDiaMenosCargadoConTurnos =
                    menosCargado.CantidadTurnos;


                resumen.PrimerTurnoPeriodo =
                    primerTurno;


                resumen.UltimoTurnoPeriodo =
                    ultimoTurno;
            }
            else
            {
                resumen
                    .PromedioTurnosPorDiaConActividad =
                    0;


                resumen.DiaMasCargado =
                    null;


                resumen.CantidadDiaMasCargado =
                    0;


                resumen.DiaMenosCargadoConTurnos =
                    null;


                resumen
                    .CantidadDiaMenosCargadoConTurnos =
                    0;


                resumen.PrimerTurnoPeriodo =
                    null;


                resumen.UltimoTurnoPeriodo =
                    null;
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
                    "La fecha hasta no puede ser "
                    +
                    "anterior a la fecha desde.");
            }


            int cantidadDias =
                (hasta.Date - desde.Date).Days
                +
                1;


            if (cantidadDias > 366)
            {
                throw new ArgumentException(
                    "El período máximo permitido "
                    +
                    "es de 366 días.");
            }
        }
    }
}
