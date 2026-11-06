using BIZ;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Consultorios
{
    public partial class PanelIAMedico
    {
        // =====================================================
        // PUNTO DE ENTRADA MÁS ALTO DEL CHAT
        //
        // Orden:
        // 1. Resumen ejecutivo
        // 2. Fechas flexibles
        // 3. Agenda inmediata
        // 4. Histórico
        // 5. Asistente general
        // =====================================================

        protected void BtnPreguntarIAEjecutivo_Click(
            object sender,
            EventArgs e)
        {
            string pregunta =
                txtPreguntaIA.Text.Trim();


            if (!EsConsultaResumenEjecutivo(
                pregunta))
            {
                BtnPreguntarIAFlexible_Click(
                    sender,
                    e);

                return;
            }


            lblMensaje.Text =
                "";


            if (string.IsNullOrWhiteSpace(
                pregunta))
            {
                lblMensaje.Text =
                    "Escribí una pregunta.";

                return;
            }


            if (pregunta.Length > 500)
            {
                lblMensaje.Text =
                    "La pregunta debe tener como máximo "
                    +
                    "500 caracteres.";

                return;
            }


            try
            {
                AsegurarProfesional();


                if (IdProfesionalActual <= 0)
                {
                    return;
                }


                string respuesta =
                    GenerarResumenEjecutivo();


                AgregarAlHistorial(
                    "Médico",
                    pregunta);


                AgregarAlHistorial(
                    "IA",
                    respuesta);


                txtPreguntaIA.Text =
                    "";


                MostrarHistorialChat();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo generar el resumen. "
                    +
                    Server.HtmlEncode(
                        ex.Message);
            }
        }


        // =====================================================
        // DETECTAR RESUMEN EJECUTIVO
        // =====================================================

        private bool EsConsultaResumenEjecutivo(
            string pregunta)
        {
            string texto =
                NormalizarTexto(
                    pregunta);


            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return false;
            }


            if (texto.Contains(
                    "que deberia saber")
                ||
                texto.Contains(
                    "que tengo que saber")
                ||
                texto.Contains(
                    "resumen ejecutivo")
                ||
                texto.Contains(
                    "resumen de mi situacion")
                ||
                texto.Contains(
                    "resumen general de mi agenda")
                ||
                texto.Contains(
                    "estado general de mi agenda")
                ||
                texto.Contains(
                    "panorama de mi agenda")
                ||
                texto.Contains(
                    "panorama general")
                ||
                texto.Contains(
                    "resumime todo")
                ||
                texto.Contains(
                    "algo importante en mi agenda")
                ||
                texto.Contains(
                    "algo importante para esta semana")
                ||
                texto.Contains(
                    "hay algo importante")
                ||
                texto.Contains(
                    "como esta todo"))
            {
                return true;
            }


            return false;
        }


        // =====================================================
        // GENERAR RESUMEN EJECUTIVO
        // =====================================================

        private string GenerarResumenEjecutivo()
        {
            StringBuilder texto =
                new StringBuilder();


            AgregarResumenFuturo(
                texto);


            texto.AppendLine();

            texto.AppendLine();


            AgregarResumenMesActual(
                texto);


            texto.AppendLine();

            texto.AppendLine();


            AgregarResumenHistoricoEjecutivo(
                texto);


            return texto.ToString();
        }


        // =====================================================
        // FUTURO CERCANO
        // =====================================================

        private void AgregarResumenFuturo(
            StringBuilder texto)
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime ahora =
                DateTime.Now;


            DateTime lunesActual =
                ObtenerLunesSemana(
                    DateTime.Today);


            DateTime proximoLunes =
                lunesActual.AddDays(7);


            DateTime lunesSiguiente =
                proximoLunes.AddDays(7);


            // =================================================
            // RESTO DE ESTA SEMANA
            // =================================================

            List<TurnoAgendaIA> restantesSemana =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    ahora,
                    proximoLunes);


            if (restantesSemana.Count == 0)
            {
                texto.Append(
                    "De acá al final de esta semana "
                    +
                    "no te quedan turnos.");
            }
            else
            {
                TurnoAgendaIA proximo =
                    restantesSemana[0];


                texto.Append(
                    "Te quedan "
                    +
                    restantesSemana.Count
                    +
                    (
                        restantesSemana.Count == 1
                            ? " turno"
                            : " turnos"
                    )
                    +
                    " esta semana. ");


                texto.Append(
                    "El próximo es el "
                    +
                    FormatearFechaResumenEjecutivo(
                        proximo.FechaHora)
                    +
                    " a las "
                    +
                    proximo.FechaHora
                        .ToString("HH:mm")
                    +
                    ".");
            }


            // =================================================
            // PRÓXIMA SEMANA
            // =================================================

            List<TurnoAgendaIA> proximaSemana =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    proximoLunes,
                    lunesSiguiente);


            texto.Append(" ");


            if (proximaSemana.Count == 0)
            {
                texto.Append(
                    "La próxima semana todavía "
                    +
                    "no tiene turnos agendados.");
            }
            else
            {
                HashSet<DateTime> dias =
                    new HashSet<DateTime>();


                foreach (
                    TurnoAgendaIA turno
                        in proximaSemana)
                {
                    dias.Add(
                        turno.FechaHora.Date);
                }


                texto.Append(
                    "Para la próxima semana ya tenés "
                    +
                    proximaSemana.Count
                    +
                    (
                        proximaSemana.Count == 1
                            ? " turno"
                            : " turnos"
                    )
                    +
                    " distribuidos en "
                    +
                    dias.Count
                    +
                    (
                        dias.Count == 1
                            ? " día."
                            : " días."
                    ));
            }


            // =================================================
            // PRÓXIMO DÍA LABORAL LIBRE
            // =================================================

            DateTime? proximoDiaLibre =
                negocio
                    .ObtenerProximoDiaLaboralSinTurnos(
                        IdProfesionalActual,
                        DateTime.Today.AddDays(1),
                        60);


            if (proximoDiaLibre.HasValue)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "Tu próximo día de trabajo "
                    +
                    "completamente libre es el "
                    +
                    FormatearFechaResumenEjecutivo(
                        proximoDiaLibre.Value)
                    +
                    ".");
            }
        }


        // =====================================================
        // MES ACTUAL
        // =====================================================

        private void AgregarResumenMesActual(
            StringBuilder texto)
        {
            DateTime hoy =
                DateTime.Today;


            DateTime desde =
                new DateTime(
                    hoy.Year,
                    hoy.Month,
                    1);


            DateTime hasta =
                desde
                    .AddMonths(1)
                    .AddDays(-1);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA resumen =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            AnalisisAgendaIA analisis =
                negocio.AnalizarAgenda(
                    IdProfesionalActual,
                    desde,
                    hasta);


            AnalisisAvanzadoAgendaNegocio negocioAvanzado =
                new AnalisisAvanzadoAgendaNegocio();


            AnalisisAvanzadoAgendaIA avanzado =
                negocioAvanzado.Analizar(
                    IdProfesionalActual,
                    desde,
                    hasta);


            string nombreMes =
                desde.ToString(
                    "MMMM",
                    new CultureInfo("es-AR"));


            // =================================================
            // ACTIVIDAD
            // =================================================

            texto.Append(
                "En "
                +
                nombreMes
                +
                " tenés "
                +
                resumen.TurnosNoCancelados
                +
                (
                    resumen.TurnosNoCancelados == 1
                        ? " turno activo"
                        : " turnos activos"
                )
                +
                ".");


            // =================================================
            // OCUPACIÓN
            // =================================================

            if (avanzado.CapacidadTeoricaTurnos > 0)
            {
                texto.Append(
                    " La ocupación de tu agenda es del "
                    +
                    FormatearPorcentajeEjecutivo(
                        avanzado.PorcentajeOcupacion)
                    +
                    ".");


                if (avanzado.PorcentajeOcupacion < 25)
                {
                    texto.Append(
                        " La agenda está bastante libre.");
                }
                else if (avanzado.PorcentajeOcupacion < 60)
                {
                    texto.Append(
                        " La carga es moderada.");
                }
                else if (avanzado.PorcentajeOcupacion < 90)
                {
                    texto.Append(
                        " La agenda está bastante ocupada.");
                }
                else
                {
                    texto.Append(
                        " La agenda está casi al límite "
                        +
                        "de la capacidad configurada.");
                }
            }


            // =================================================
            // DÍAS SIN TURNOS
            // =================================================

            if (avanzado.DiasLaborables > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                if (avanzado.DiasLaborablesSinTurnos == 0)
                {
                    texto.Append(
                        "Todos tus días laborales del mes "
                        +
                        "tienen al menos un turno.");
                }
                else
                {
                    texto.Append(
                        "De tus "
                        +
                        avanzado.DiasLaborables
                        +
                        " días laborales del mes, "
                        +
                        avanzado.DiasLaborablesSinTurnos
                        +
                        (
                            avanzado.DiasLaborablesSinTurnos == 1
                                ? " todavía no tiene turnos."
                                : " todavía no tienen turnos."
                        ));
                }
            }


            // =================================================
            // ANOMALÍAS
            // =================================================

            texto.AppendLine();

            texto.AppendLine();


            if (analisis.Superposiciones == 0
                &&
                analisis.TurnosFueraHorario == 0
                &&
                analisis.TurnosEnDiaNoLaborable == 0)
            {
                texto.Append(
                    "No detecté superposiciones ni "
                    +
                    "turnos fuera de tus días "
                    +
                    "u horarios de trabajo.");
            }
            else
            {
                List<string> problemas =
                    new List<string>();


                if (analisis.Superposiciones > 0)
                {
                    problemas.Add(
                        analisis.Superposiciones
                        +
                        (
                            analisis.Superposiciones == 1
                                ? " superposición"
                                : " superposiciones"
                        ));
                }


                if (analisis.TurnosFueraHorario > 0)
                {
                    problemas.Add(
                        analisis.TurnosFueraHorario
                        +
                        (
                            analisis.TurnosFueraHorario == 1
                                ? " turno fuera de horario"
                                : " turnos fuera de horario"
                        ));
                }


                if (analisis.TurnosEnDiaNoLaborable > 0)
                {
                    problemas.Add(
                        analisis.TurnosEnDiaNoLaborable
                        +
                        (
                            analisis.TurnosEnDiaNoLaborable == 1
                                ? " turno en un día no laborable"
                                : " turnos en días no laborables"
                        ));
                }


                texto.Append(
                    "Hay algo para revisar: "
                    +
                    UnirListaEjecutiva(
                        problemas)
                    +
                    ".");
            }


            // =================================================
            // HUECOS
            // =================================================

            if (avanzado.HuecosLargos > 0)
            {
                texto.Append(
                    " También detecté "
                    +
                    avanzado.HuecosLargos
                    +
                    (
                        avanzado.HuecosLargos == 1
                            ? " hueco de una hora o más"
                            : " huecos de una hora o más"
                    )
                    +
                    " entre turnos.");


                if (avanzado.MayorHuecoMinutos > 0)
                {
                    texto.Append(
                        " El mayor es de "
                        +
                        avanzado.MayorHuecoMinutos
                        +
                        " minutos.");
                }
            }


            // =================================================
            // CONCENTRACIÓN
            // =================================================

            if (resumen.TurnosNoCancelados >= 4
                &&
                resumen.DiaMasCargado.HasValue)
            {
                decimal concentracion =
                    (
                        (decimal)
                        resumen.CantidadDiaMasCargado
                        /
                        resumen.TurnosNoCancelados
                    )
                    *
                    100;


                if (concentracion >= 50)
                {
                    texto.AppendLine();

                    texto.AppendLine();


                    texto.Append(
                        "La carga está bastante concentrada: "
                        +
                        resumen.CantidadDiaMasCargado
                        +
                        " de tus "
                        +
                        resumen.TurnosNoCancelados
                        +
                        " turnos activos están el "
                        +
                        FormatearFechaResumenEjecutivo(
                            resumen
                                .DiaMasCargado
                                .Value)
                        +
                        ".");
                }
            }
        }


        // =====================================================
        // HISTÓRICO
        // =====================================================

        private void AgregarResumenHistoricoEjecutivo(
            StringBuilder texto)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    6);


            if (resumen.TotalTurnos == 0)
            {
                texto.Append(
                    "Todavía no hay actividad registrada "
                    +
                    "en los últimos 6 meses como para "
                    +
                    "hacer un análisis histórico.");

                return;
            }


            texto.Append(
                "En los últimos 6 meses registraste "
                +
                resumen.TotalTurnos
                +
                (
                    resumen.TotalTurnos == 1
                        ? " turno"
                        : " turnos"
                )
                +
                ". ");


            if (resumen.Atendidos == 0)
            {
                texto.Append(
                    "Todavía ninguno está marcado "
                    +
                    "como atendido.");
            }
            else if (resumen.Atendidos == 1)
            {
                texto.Append(
                    "Uno terminó atendido.");
            }
            else
            {
                texto.Append(
                    resumen.Atendidos
                    +
                    " terminaron atendidos.");
            }


            texto.Append(
                " Tu tasa de atención es del "
                +
                FormatearPorcentajeEjecutivo(
                    resumen.TasaAtencion)
                +
                " y la de cancelación del "
                +
                FormatearPorcentajeEjecutivo(
                    resumen.TasaCancelacion)
                +
                ".");


            // =================================================
            // CUÁNTOS MESES TIENEN ACTIVIDAD
            // =================================================

            int mesesConActividad =
                0;


            MesActividadHistoricaIA primerActivo =
                null;


            MesActividadHistoricaIA ultimoActivo =
                null;


            foreach (
                MesActividadHistoricaIA mes
                    in resumen.Meses)
            {
                if (mes.TurnosNoCancelados > 0)
                {
                    mesesConActividad++;


                    if (primerActivo == null)
                    {
                        primerActivo =
                            mes;
                    }


                    ultimoActivo =
                        mes;
                }
            }


            texto.AppendLine();

            texto.AppendLine();


            if (mesesConActividad <= 1)
            {
                texto.Append(
                    "Todavía no hay suficiente historial "
                    +
                    "para identificar una tendencia "
                    +
                    "confiable.");
            }
            else if (mesesConActividad == 2)
            {
                texto.Append(
                    "Ya hay actividad en dos meses, "
                    +
                    "pero todavía es poco historial "
                    +
                    "para hablar de una tendencia sostenida.");


                if (primerActivo != null
                    &&
                    ultimoActivo != null)
                {
                    texto.Append(
                        " Pasaste de "
                        +
                        primerActivo.TurnosNoCancelados
                        +
                        " turnos activos en "
                        +
                        primerActivo.NombrePeriodo
                        +
                        " a "
                        +
                        ultimoActivo.TurnosNoCancelados
                        +
                        " en "
                        +
                        ultimoActivo.NombrePeriodo
                        +
                        ".");
                }
            }
            else
            {
                if (primerActivo != null
                    &&
                    ultimoActivo != null)
                {
                    if (ultimoActivo.TurnosNoCancelados >
                        primerActivo.TurnosNoCancelados)
                    {
                        texto.Append(
                            "Tomando los meses con actividad, "
                            +
                            "la tendencia general viene "
                            +
                            "en aumento.");
                    }
                    else if (ultimoActivo.TurnosNoCancelados <
                             primerActivo.TurnosNoCancelados)
                    {
                        texto.Append(
                            "Tomando los meses con actividad, "
                            +
                            "la tendencia general viene "
                            +
                            "en descenso.");
                    }
                    else
                    {
                        texto.Append(
                            "La actividad se mantiene bastante "
                            +
                            "estable entre el primer y el "
                            +
                            "último mes con registros.");
                    }
                }
            }


            // =================================================
            // MES MÁS ACTIVO
            // =================================================

            if (resumen.MesMasActivo != null
                &&
                resumen.MesMasActivo
                    .TurnosNoCancelados > 0)
            {
                texto.Append(
                    " El mes con más actividad fue "
                    +
                    resumen.MesMasActivo.NombrePeriodo
                    +
                    ", con "
                    +
                    resumen.MesMasActivo
                        .TurnosNoCancelados
                    +
                    (
                        resumen.MesMasActivo
                            .TurnosNoCancelados == 1
                            ? " turno activo."
                            : " turnos activos."
                    ));
            }
        }


        // =====================================================
        // FECHA NATURAL
        // =====================================================

        private string FormatearFechaResumenEjecutivo(
            DateTime fecha)
        {
            return fecha.ToString(
                "dddd d 'de' MMMM",
                new CultureInfo("es-AR"));
        }


        // =====================================================
        // PORCENTAJE
        // =====================================================

        private string FormatearPorcentajeEjecutivo(
            decimal valor)
        {
            return valor.ToString(
                "0.0",
                new CultureInfo("es-AR"))
                +
                "%";
        }


        // =====================================================
        // LISTA NATURAL
        // =====================================================

        private string UnirListaEjecutiva(
            List<string> elementos)
        {
            if (elementos == null
                ||
                elementos.Count == 0)
            {
                return "";
            }


            if (elementos.Count == 1)
            {
                return elementos[0];
            }


            if (elementos.Count == 2)
            {
                return
                    elementos[0]
                    +
                    " y "
                    +
                    elementos[1];
            }


            StringBuilder texto =
                new StringBuilder();


            for (int i = 0;
                 i < elementos.Count;
                 i++)
            {
                if (i ==
                    elementos.Count - 1)
                {
                    texto.Append(
                        " y "
                        +
                        elementos[i]);
                }
                else if (i == 0)
                {
                    texto.Append(
                        elementos[i]);
                }
                else
                {
                    texto.Append(
                        ", "
                        +
                        elementos[i]);
                }
            }


            return texto.ToString();
        }
    }
}
