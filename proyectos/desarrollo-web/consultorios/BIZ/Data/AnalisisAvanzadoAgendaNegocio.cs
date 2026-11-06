using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class AnalisisAvanzadoAgendaNegocio
    {
        // =====================================================
        // CLASES INTERNAS
        // =====================================================

        private class TurnoInterno
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


        private class HorarioInterno
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


            public int DuracionMinutos
            {
                get;
                set;
            }


            public bool AtencionConTurno
            {
                get;
                set;
            }
        }


        // =====================================================
        // ANALIZAR
        // =====================================================

        public AnalisisAvanzadoAgendaIA Analizar(
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


            AnalisisAvanzadoAgendaIA resultado =
                new AnalisisAvanzadoAgendaIA();


            resultado.Desde =
                desde;


            resultado.Hasta =
                hasta;


            List<TurnoInterno> turnos =
                ObtenerTurnos(
                    idProfesional,
                    desde,
                    hasta);


            List<HorarioInterno> horarios =
                ObtenerHorarios(
                    idProfesional);


            DateTime fecha =
                desde;


            while (fecha <=
                   hasta)
            {
                int numeroDia =
                    ConvertirDiaSemana(
                        fecha.DayOfWeek);


                List<HorarioInterno> horariosDia =
                    ObtenerHorariosDia(
                        horarios,
                        numeroDia);


                // =============================================
                // SI EL MÉDICO NO TRABAJA ESE DÍA
                // NO LO CONTAMOS COMO DÍA LIBRE
                // =============================================

                if (horariosDia.Count == 0)
                {
                    fecha =
                        fecha.AddDays(1);

                    continue;
                }


                resultado.DiasLaborables++;


                List<TurnoInterno> turnosDia =
                    ObtenerTurnosDia(
                        turnos,
                        fecha);


                // =============================================
                // DÍA LABORABLE SIN TURNOS
                // =============================================

                if (turnosDia.Count == 0)
                {
                    resultado
                        .DiasLaborablesSinTurnos++;


                    AlertaAvanzadaAgendaIA alerta =
                        new AlertaAvanzadaAgendaIA();


                    alerta.Tipo =
                        "DIA_LABORABLE_SIN_TURNOS";


                    alerta.FechaHora =
                        fecha;


                    alerta.Descripcion =
                        "El "
                        +
                        fecha.ToString("dd/MM/yyyy")
                        +
                        " está configurado como día "
                        +
                        "laborable pero no tiene "
                        +
                        "turnos activos.";


                    resultado.Alertas.Add(
                        alerta);
                }


                // =============================================
                // CAPACIDAD TEÓRICA
                // =============================================

                foreach (
                    HorarioInterno horario
                        in horariosDia)
                {
                    if (!horario.AtencionConTurno)
                    {
                        continue;
                    }


                    if (horario.DuracionMinutos <= 0)
                    {
                        continue;
                    }


                    double minutosDisponibles =
                        (
                            horario.HoraFin
                            -
                            horario.HoraInicio
                        )
                        .TotalMinutes;


                    if (minutosDisponibles <= 0)
                    {
                        continue;
                    }


                    int capacidad =
                        (int)Math.Floor(
                            minutosDisponibles
                            /
                            horario.DuracionMinutos);


                    resultado.CapacidadTeoricaTurnos +=
                        capacidad;
                }


                // =============================================
                // TURNOS REALMENTE DENTRO DEL HORARIO
                // =============================================

                foreach (
                    TurnoInterno turno
                        in turnosDia)
                {
                    if (EstaDentroDeHorario(
                        turno,
                        horariosDia))
                    {
                        resultado
                            .TurnosDentroHorarioLaboral++;
                    }
                }


                // =============================================
                // HUECOS ENTRE TURNOS
                // =============================================

                turnosDia.Sort(
                    delegate (
                        TurnoInterno primero,
                        TurnoInterno segundo)
                    {
                        return primero.FechaHora
                            .CompareTo(
                                segundo.FechaHora);
                    });


                for (int i = 0;
                     i < turnosDia.Count - 1;
                     i++)
                {
                    TurnoInterno actual =
                        turnosDia[i];


                    TurnoInterno siguiente =
                        turnosDia[i + 1];


                    double diferencia =
                        (
                            siguiente.FechaHora
                            -
                            actual.FechaFin
                        )
                        .TotalMinutes;


                    // Si es negativo hay superposición.
                    // Eso ya lo analiza el otro motor.

                    if (diferencia < 60)
                    {
                        continue;
                    }


                    int minutosHueco =
                        Convert.ToInt32(
                            Math.Floor(
                                diferencia));


                    resultado.HuecosLargos++;


                    resultado.MinutosHuecosTotales +=
                        minutosHueco;


                    if (minutosHueco >
                        resultado.MayorHuecoMinutos)
                    {
                        resultado.MayorHuecoMinutos =
                            minutosHueco;


                        resultado.FechaMayorHueco =
                            actual.FechaHora;
                    }


                    AlertaAvanzadaAgendaIA alerta =
                        new AlertaAvanzadaAgendaIA();


                    alerta.Tipo =
                        "HUECO_LARGO";


                    alerta.FechaHora =
                        actual.FechaHora;


                    alerta.Descripcion =
                        "Se detectó un hueco de "
                        +
                        minutosHueco
                        +
                        " minutos el "
                        +
                        actual.FechaHora.ToString(
                            "dd/MM/yyyy")
                        +
                        ", entre las "
                        +
                        actual.FechaFin.ToString(
                            "HH:mm")
                        +
                        " y las "
                        +
                        siguiente.FechaHora.ToString(
                            "HH:mm")
                        +
                        ".";


                    resultado.Alertas.Add(
                        alerta);
                }


                fecha =
                    fecha.AddDays(1);
            }


            // =================================================
            // PORCENTAJE DE OCUPACIÓN
            // =================================================

            if (resultado.CapacidadTeoricaTurnos > 0)
            {
                decimal porcentaje =
                    (
                        (decimal)
                        resultado.TurnosDentroHorarioLaboral
                        /
                        resultado.CapacidadTeoricaTurnos
                    )
                    *
                    100;


                resultado.PorcentajeOcupacion =
                    Math.Round(
                        porcentaje,
                        1);
            }
            else
            {
                resultado.PorcentajeOcupacion =
                    0;
            }


            return resultado;
        }


        // =====================================================
        // TURNOS DEL PERÍODO
        // =====================================================

        private List<TurnoInterno> ObtenerTurnos(
            int idProfesional,
            DateTime desde,
            DateTime hasta)
        {
            List<TurnoInterno> lista =
                new List<TurnoInterno>();


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
                            TurnoInterno turno =
                                new TurnoInterno();


                            turno.IdTurno =
                                Convert.ToInt32(
                                    lector[
                                        "IdTurno"]);


                            turno.FechaHora =
                                Convert.ToDateTime(
                                    lector[
                                        "FechaHora"]);


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
        // AGENDA ACTIVA
        // =====================================================

        private List<HorarioInterno> ObtenerHorarios(
            int idProfesional)
        {
            List<HorarioInterno> lista =
                new List<HorarioInterno>();


            using (SqlConnection conexion =
                new SqlConnection(
                    ConfigurationManager
                        .ConnectionStrings["Conexion"]
                        .ConnectionString))
            {
                string sql =
                    @"
                    SELECT
                        A.DiaSemana,
                        A.HoraInicio,
                        A.HoraFin,
                        A.DuracionMinutos,
                        A.AtencionConTurno

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
                            HorarioInterno horario =
                                new HorarioInterno();


                            horario.DiaSemana =
                                Convert.ToInt32(
                                    lector[
                                        "DiaSemana"]);


                            horario.HoraInicio =
                                (TimeSpan)
                                lector[
                                    "HoraInicio"];


                            horario.HoraFin =
                                (TimeSpan)
                                lector[
                                    "HoraFin"];


                            horario.DuracionMinutos =
                                Convert.ToInt32(
                                    lector[
                                        "DuracionMinutos"]);


                            horario.AtencionConTurno =
                                Convert.ToBoolean(
                                    lector[
                                        "AtencionConTurno"]);


                            lista.Add(
                                horario);
                        }
                    }
                }
            }


            return lista;
        }


        // =====================================================
        // HORARIOS DE UN DÍA
        // =====================================================

        private List<HorarioInterno> ObtenerHorariosDia(
            List<HorarioInterno> horarios,
            int diaSemana)
        {
            List<HorarioInterno> resultado =
                new List<HorarioInterno>();


            foreach (
                HorarioInterno horario
                    in horarios)
            {
                if (horario.DiaSemana ==
                    diaSemana)
                {
                    resultado.Add(
                        horario);
                }
            }


            return resultado;
        }


        // =====================================================
        // TURNOS DE UNA FECHA
        // =====================================================

        private List<TurnoInterno> ObtenerTurnosDia(
            List<TurnoInterno> turnos,
            DateTime fecha)
        {
            List<TurnoInterno> resultado =
                new List<TurnoInterno>();


            foreach (
                TurnoInterno turno
                    in turnos)
            {
                if (turno.FechaHora.Date ==
                    fecha.Date)
                {
                    resultado.Add(
                        turno);
                }
            }


            return resultado;
        }


        // =====================================================
        // TURNO DENTRO DEL HORARIO
        // =====================================================

        private bool EstaDentroDeHorario(
            TurnoInterno turno,
            List<HorarioInterno> horarios)
        {
            TimeSpan inicio =
                turno.FechaHora.TimeOfDay;


            TimeSpan fin =
                turno.FechaFin.TimeOfDay;


            foreach (
                HorarioInterno horario
                    in horarios)
            {
                if (inicio >=
                        horario.HoraInicio
                    &&
                    fin <=
                        horario.HoraFin)
                {
                    return true;
                }
            }


            return false;
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
        // VALIDAR
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


            if (dias > 366)
            {
                throw new ArgumentException(
                    "El período máximo permitido "
                    +
                    "es de 366 días.");
            }
        }
    }
}
