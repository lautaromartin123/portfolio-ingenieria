using BIZ;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Consultorios
{
    public partial class PanelIAMedico
    {
        // =====================================================
        // ENTRADA DEL CHAT HISTORICO
        // =====================================================

        protected void BtnPreguntarIAHistorico_Click(
            object sender,
            EventArgs e)
        {
            string pregunta =
                txtPreguntaIA.Text.Trim();


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


            // =================================================
            // ENTRADA DEMASIADO CORTA
            // =================================================

            if (EsEntradaDemasiadoCorta(
                pregunta))
            {
                AgregarAlHistorial(
                    "Médico",
                    pregunta);


                AgregarAlHistorial(
                    "IA",
                    "No llegué a entender la consulta. "
                    +
                    "Podés preguntarme, por ejemplo, "
                    +
                    "sobre tus turnos, horarios, ocupación, "
                    +
                    "agenda o estadísticas históricas.");


                txtPreguntaIA.Text =
                    "";


                MostrarHistorialChat();

                return;
            }


            // =================================================
            // SI NO ES HISTORICA, SIGUE AL CHAT GENERAL
            // =================================================

            if (!EsConsultaHistorica(
                pregunta))
            {
                BtnPreguntarIA_Click(
                    sender,
                    e);

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
                    EjecutarConsultaHistorica(
                        pregunta);


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
                    "No se pudo procesar la consulta histórica. "
                    +
                    Server.HtmlEncode(
                        ex.Message);
            }
        }


        private bool EsEntradaDemasiadoCorta(
            string pregunta)
        {
            if (string.IsNullOrWhiteSpace(
                pregunta))
            {
                return true;
            }


            return pregunta.Trim().Length < 3;
        }


        // =====================================================
        // DETECTAR HISTORICO
        // =====================================================

        private bool EsConsultaHistorica(
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
                    "tasa de atencion")
                ||
                texto.Contains(
                    "tasa de asistencia")
                ||
                texto.Contains(
                    "porcentaje de mis turnos termina atendido")
                ||
                texto.Contains(
                    "porcentaje de turnos termina atendido")
                ||
                texto.Contains(
                    "porcentaje termina atendido"))
            {
                return true;
            }


            if (texto.Contains(
                    "tasa de cancelacion")
                ||
                texto.Contains(
                    "porcentaje de cancelacion")
                ||
                texto.Contains(
                    "porcentaje se cancela")
                ||
                texto.Contains(
                    "cuantos se cancelan normalmente"))
            {
                return true;
            }


            if (texto.Contains(
                    "dia de la semana")
                &&
                (
                    texto.Contains(
                        "mas pacientes")
                    ||
                    texto.Contains(
                        "mas atendidos")
                    ||
                    texto.Contains(
                        "mas actividad")
                    ||
                    texto.Contains(
                        "mas demanda")
                ))
            {
                return true;
            }


            if (texto.Contains(
                    "este mes")
                &&
                (
                    texto.Contains(
                        "mes pasado")
                    ||
                    texto.Contains(
                        "mes anterior")
                )
                &&
                (
                    texto.Contains(
                        "atendi")
                    ||
                    texto.Contains(
                        "atendidos")
                    ||
                    texto.Contains(
                        "pacientes")
                ))
            {
                return true;
            }


            if (texto.Contains(
                    "evolucionando mi actividad")
                ||
                texto.Contains(
                    "evolucion de mi actividad")
                ||
                texto.Contains(
                    "evoluciono mi actividad")
                ||
                texto.Contains(
                    "tendencia de mi actividad")
                ||
                texto.Contains(
                    "como viene mi actividad")
                ||
                texto.Contains(
                    "actividad ultimamente"))
            {
                return true;
            }


            if (texto.Contains(
                    "mes con mas trabajo")
                ||
                texto.Contains(
                    "mes con mas actividad")
                ||
                texto.Contains(
                    "mes mas activo")
                ||
                texto.Contains(
                    "mes trabaje mas"))
            {
                return true;
            }


            if (texto.Contains(
                    "ultimos")
                &&
                texto.Contains(
                    "meses")
                &&
                (
                    texto.Contains(
                        "actividad")
                    ||
                    texto.Contains(
                        "resumi")
                    ||
                    texto.Contains(
                        "resumen")
                ))
            {
                return true;
            }


            return false;
        }


        // =====================================================
        // ROUTER
        // =====================================================

        private string EjecutarConsultaHistorica(
            string pregunta)
        {
            string texto =
                NormalizarTexto(
                    pregunta);


            if (texto.Contains(
                    "tasa de atencion")
                ||
                texto.Contains(
                    "tasa de asistencia")
                ||
                texto.Contains(
                    "termina atendido"))
            {
                return ResponderTasaAtencionHistorica(
                    ObtenerCantidadMeses(
                        texto,
                        6));
            }


            if (texto.Contains(
                    "tasa de cancelacion")
                ||
                texto.Contains(
                    "porcentaje de cancelacion")
                ||
                texto.Contains(
                    "se cancelan normalmente"))
            {
                return ResponderTasaCancelacionHistorica(
                    ObtenerCantidadMeses(
                        texto,
                        6));
            }


            if (texto.Contains(
                    "dia de la semana")
                &&
                (
                    texto.Contains(
                        "mas pacientes")
                    ||
                    texto.Contains(
                        "mas atendidos")
                    ||
                    texto.Contains(
                        "mas demanda")
                    ||
                    texto.Contains(
                        "mas actividad")
                ))
            {
                return ResponderDiaHistoricoMasAtendido(
                    ObtenerCantidadMeses(
                        texto,
                        6));
            }


            if (texto.Contains(
                    "este mes")
                &&
                (
                    texto.Contains(
                        "mes pasado")
                    ||
                    texto.Contains(
                        "mes anterior")
                ))
            {
                return ResponderComparacionMesesAtendidos();
            }


            if (texto.Contains(
                    "evolucion")
                ||
                texto.Contains(
                    "tendencia")
                ||
                texto.Contains(
                    "como viene mi actividad")
                ||
                texto.Contains(
                    "actividad ultimamente"))
            {
                return ResponderEvolucionHistorica(
                    ObtenerCantidadMeses(
                        texto,
                        6));
            }


            if (texto.Contains(
                    "mes con mas trabajo")
                ||
                texto.Contains(
                    "mes con mas actividad")
                ||
                texto.Contains(
                    "mes mas activo")
                ||
                texto.Contains(
                    "mes trabaje mas"))
            {
                return ResponderMesMasActivo(
                    ObtenerCantidadMeses(
                        texto,
                        12));
            }


            return ResponderResumenHistorico(
                ObtenerCantidadMeses(
                    texto,
                    6));
        }


        // =====================================================
        // TASA ATENCION
        // =====================================================

        private string ResponderTasaAtencionHistorica(
            int cantidadMeses)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    cantidadMeses);


            if (resumen.TotalTurnos == 0)
            {
                return
                    "No tenés turnos registrados en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, así que todavía no puedo calcular "
                    +
                    "una tasa de atención.";
            }


            StringBuilder texto =
                new StringBuilder();


            if (resumen.Atendidos == 0)
            {
                texto.Append(
                    "En los últimos "
                    +
                    cantidadMeses
                    +
                    " meses todavía no tenés turnos "
                    +
                    "marcados como atendidos. ");
            }
            else if (resumen.Atendidos == 1)
            {
                texto.Append(
                    "En los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, 1 de tus "
                    +
                    resumen.TotalTurnos
                    +
                    " turnos terminó atendido. ");
            }
            else
            {
                texto.Append(
                    "En los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, "
                    +
                    resumen.Atendidos
                    +
                    " de tus "
                    +
                    resumen.TotalTurnos
                    +
                    " turnos terminaron atendidos. ");
            }


            texto.Append(
                "Eso representa una tasa de atención del "
                +
                FormatearPorcentajeHistorico(
                    resumen.TasaAtencion)
                +
                ".");


            if (resumen.Cancelados > 0)
            {
                texto.Append(
                    " Si dejamos afuera los turnos cancelados, "
                    +
                    "la tasa queda en "
                    +
                    FormatearPorcentajeHistorico(
                        resumen.TasaAtencionSobreNoCancelados)
                    +
                    ".");
            }


            return texto.ToString();
        }


        // =====================================================
        // CANCELACION
        // =====================================================

        private string ResponderTasaCancelacionHistorica(
            int cantidadMeses)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    cantidadMeses);


            if (resumen.TotalTurnos == 0)
            {
                return
                    "No tenés turnos registrados en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, así que todavía no puedo calcular "
                    +
                    "una tasa de cancelación.";
            }


            if (resumen.Cancelados == 0)
            {
                return
                    "En los últimos "
                    +
                    cantidadMeses
                    +
                    " meses no tuviste turnos cancelados. "
                    +
                    "Tu tasa de cancelación es del 0%.";
            }


            if (resumen.Cancelados == 1)
            {
                return
                    "En los últimos "
                    +
                    cantidadMeses
                    +
                    " meses se canceló 1 de tus "
                    +
                    resumen.TotalTurnos
                    +
                    " turnos. Tu tasa de cancelación es del "
                    +
                    FormatearPorcentajeHistorico(
                        resumen.TasaCancelacion)
                    +
                    ".";
            }


            return
                "En los últimos "
                +
                cantidadMeses
                +
                " meses se cancelaron "
                +
                resumen.Cancelados
                +
                " de tus "
                +
                resumen.TotalTurnos
                +
                " turnos. Tu tasa de cancelación es del "
                +
                FormatearPorcentajeHistorico(
                    resumen.TasaCancelacion)
                +
                ".";
        }


        // =====================================================
        // DIA HISTORICO
        // =====================================================

        private string ResponderDiaHistoricoMasAtendido(
            int cantidadMeses)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    cantidadMeses);


            if (resumen.Atendidos == 0)
            {
                return
                    "Todavía no tenés pacientes registrados "
                    +
                    "como atendidos en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, así que no hay un patrón "
                    +
                    "para comparar.";
            }


            int maximo =
                0;


            foreach (
                DiaSemanaHistoricoIA dia
                    in resumen.DiasSemana)
            {
                if (dia.Atendidos >
                    maximo)
                {
                    maximo =
                        dia.Atendidos;
                }
            }


            List<string> dias =
                new List<string>();


            foreach (
                DiaSemanaHistoricoIA dia
                    in resumen.DiasSemana)
            {
                if (dia.Atendidos ==
                        maximo
                    &&
                    maximo > 0)
                {
                    dias.Add(
                        dia.NombreDia);
                }
            }


            if (resumen.Atendidos < 3)
            {
                if (dias.Count == 1)
                {
                    return
                        "Con los datos actuales, el "
                        +
                        dias[0]
                        +
                        " es el día que tiene más pacientes "
                        +
                        "atendidos, con "
                        +
                        maximo
                        +
                        (
                            maximo == 1
                                ? " atención registrada. "
                                : " atenciones registradas. "
                        )
                        +
                        "Todavía hay poco historial para decir "
                        +
                        "que sea un patrón habitual.";
                }


                return
                    "Con los pocos datos disponibles hay un empate "
                    +
                    "entre "
                    +
                    UnirListaHistorica(
                        dias)
                    +
                    ". Todavía no hay suficiente historial "
                    +
                    "para identificar un día habitual.";
            }


            if (dias.Count == 1)
            {
                return
                    "Tomando los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, el "
                    +
                    dias[0]
                    +
                    " es el día en el que más pacientes "
                    +
                    "terminaste atendiendo, con "
                    +
                    maximo
                    +
                    " atenciones registradas.";
            }


            return
                "En los últimos "
                +
                cantidadMeses
                +
                " meses hay un empate entre "
                +
                UnirListaHistorica(
                    dias)
                +
                ": cada uno tiene "
                +
                maximo
                +
                " pacientes atendidos.";
        }


        // =====================================================
        // MES ACTUAL VS ANTERIOR
        // =====================================================

        private string ResponderComparacionMesesAtendidos()
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            DateTime hoy =
                DateTime.Today;


            DateTime inicioActual =
                new DateTime(
                    hoy.Year,
                    hoy.Month,
                    1);


            DateTime finActual =
                hoy.Date;


            DateTime inicioAnterior =
                inicioActual.AddMonths(-1);


            DateTime finAnterior =
                inicioActual.AddDays(-1);


            ResumenHistoricoIA actual =
                negocio.ObtenerResumen(
                    IdProfesionalActual,
                    inicioActual,
                    finActual);


            ResumenHistoricoIA anterior =
                negocio.ObtenerResumen(
                    IdProfesionalActual,
                    inicioAnterior,
                    finAnterior);


            int diferencia =
                actual.Atendidos
                -
                anterior.Atendidos;


            string nombreActual =
                inicioActual.ToString(
                    "MMMM",
                    new CultureInfo("es-AR"));


            string nombreAnterior =
                inicioAnterior.ToString(
                    "MMMM",
                    new CultureInfo("es-AR"));


            if (diferencia > 0)
            {
                return
                    "Sí. Hasta ahora en "
                    +
                    nombreActual
                    +
                    " atendiste "
                    +
                    actual.Atendidos
                    +
                    (
                        actual.Atendidos == 1
                            ? " paciente"
                            : " pacientes"
                    )
                    +
                    ", mientras que en "
                    +
                    nombreAnterior
                    +
                    " atendiste "
                    +
                    anterior.Atendidos
                    +
                    ". Son "
                    +
                    diferencia
                    +
                    (
                        diferencia == 1
                            ? " atención más."
                            : " atenciones más."
                    );
            }


            if (diferencia < 0)
            {
                return
                    "No. Hasta ahora en "
                    +
                    nombreActual
                    +
                    " atendiste "
                    +
                    actual.Atendidos
                    +
                    ", frente a "
                    +
                    anterior.Atendidos
                    +
                    " en "
                    +
                    nombreAnterior
                    +
                    ". Son "
                    +
                    Math.Abs(
                        diferencia)
                    +
                    " menos.";
            }


            return
                "Hasta ahora atendiste la misma cantidad "
                +
                "en ambos meses: "
                +
                actual.Atendidos
                +
                " en "
                +
                nombreActual
                +
                " y "
                +
                anterior.Atendidos
                +
                " en "
                +
                nombreAnterior
                +
                ".";
        }


        // =====================================================
        // EVOLUCION
        // =====================================================

        private string ResponderEvolucionHistorica(
            int cantidadMeses)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    cantidadMeses);


            if (resumen.TotalTurnos == 0)
            {
                return
                    "No hay actividad registrada en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses, así que todavía no hay una "
                    +
                    "tendencia para analizar.";
            }


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "En los últimos "
                +
                cantidadMeses
                +
                " meses tu actividad fue: ");


            List<string> detalle =
                new List<string>();


            int mesesConActividad =
                0;


            foreach (
                MesActividadHistoricaIA mes
                    in resumen.Meses)
            {
                detalle.Add(
                    mes.NombreMes
                    +
                    ": "
                    +
                    mes.TurnosNoCancelados);


                if (mes.TurnosNoCancelados > 0)
                {
                    mesesConActividad++;
                }
            }


            texto.Append(
                UnirListaHistorica(
                    detalle));


            texto.Append(".");


            // =================================================
            // UN SOLO MES CON ACTIVIDAD
            // =================================================

            if (mesesConActividad == 1)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "Todavía no hay suficiente historial "
                    +
                    "para hablar de una tendencia: "
                    +
                    "solo uno de esos meses tiene "
                    +
                    "actividad registrada.");


                if (resumen.MesMasActivo != null)
                {
                    texto.Append(
                        " Ese mes es "
                        +
                        resumen.MesMasActivo.NombrePeriodo
                        +
                        ", con "
                        +
                        FormatearTurnosActivosHistorico(
                            resumen.MesMasActivo
                                .TurnosNoCancelados)
                        +
                        ".");
                }


                return texto.ToString();
            }


            // =================================================
            // DOS MESES
            // =================================================

            if (mesesConActividad == 2)
            {
                MesActividadHistoricaIA primero =
                    ObtenerPrimerMesConActividad(
                        resumen);


                MesActividadHistoricaIA ultimo =
                    ObtenerUltimoMesConActividad(
                        resumen);


                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "Ya hay dos meses con actividad, "
                    +
                    "pero todavía es poco para hablar de "
                    +
                    "una tendencia sostenida. ");


                if (primero != null &&
                    ultimo != null)
                {
                    texto.Append(
                        "Pasaste de "
                        +
                        FormatearTurnosActivosHistorico(
                            primero.TurnosNoCancelados)
                        +
                        " en "
                        +
                        primero.NombrePeriodo
                        +
                        " a "
                        +
                        FormatearTurnosActivosHistorico(
                            ultimo.TurnosNoCancelados)
                        +
                        " en "
                        +
                        ultimo.NombrePeriodo
                        +
                        ".");
                }


                return texto.ToString();
            }


            // =================================================
            // TRES O MAS MESES
            // =================================================

            MesActividadHistoricaIA primerActivo =
                ObtenerPrimerMesConActividad(
                    resumen);


            MesActividadHistoricaIA ultimoActivo =
                ObtenerUltimoMesConActividad(
                    resumen);


            if (primerActivo != null &&
                ultimoActivo != null)
            {
                texto.AppendLine();

                texto.AppendLine();


                if (ultimoActivo.TurnosNoCancelados >
                    primerActivo.TurnosNoCancelados)
                {
                    texto.Append(
                        "Tomando los meses con actividad, "
                        +
                        "la tendencia general es al alza: "
                        +
                        "pasaste de "
                        +
                        FormatearTurnosActivosHistorico(
                            primerActivo
                                .TurnosNoCancelados)
                        +
                        " en "
                        +
                        primerActivo.NombrePeriodo
                        +
                        " a "
                        +
                        FormatearTurnosActivosHistorico(
                            ultimoActivo
                                .TurnosNoCancelados)
                        +
                        " en "
                        +
                        ultimoActivo.NombrePeriodo
                        +
                        ".");
                }
                else if (ultimoActivo.TurnosNoCancelados <
                         primerActivo.TurnosNoCancelados)
                {
                    texto.Append(
                        "Tomando los meses con actividad, "
                        +
                        "la tendencia general es a la baja: "
                        +
                        "pasaste de "
                        +
                        FormatearTurnosActivosHistorico(
                            primerActivo
                                .TurnosNoCancelados)
                        +
                        " en "
                        +
                        primerActivo.NombrePeriodo
                        +
                        " a "
                        +
                        FormatearTurnosActivosHistorico(
                            ultimoActivo
                                .TurnosNoCancelados)
                        +
                        " en "
                        +
                        ultimoActivo.NombrePeriodo
                        +
                        ".");
                }
                else
                {
                    texto.Append(
                        "El primer y el último mes con actividad "
                        +
                        "tienen la misma cantidad: "
                        +
                        FormatearTurnosActivosHistorico(
                            ultimoActivo
                                .TurnosNoCancelados)
                        +
                        ".");
                }
            }


            if (resumen.MesMasActivo != null &&
                resumen.MesMasActivo
                    .TurnosNoCancelados > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "El mes con más actividad fue "
                    +
                    resumen.MesMasActivo.NombrePeriodo
                    +
                    ", con "
                    +
                    FormatearTurnosActivosHistorico(
                        resumen.MesMasActivo
                            .TurnosNoCancelados)
                    +
                    ".");
            }


            return texto.ToString();
        }


        // =====================================================
        // SINGULAR / PLURAL
        // =====================================================

        private string FormatearTurnosActivosHistorico(
            int cantidad)
        {
            if (cantidad == 1)
            {
                return "1 turno activo";
            }


            return
                cantidad
                +
                " turnos activos";
        }


        // =====================================================
        // PRIMER MES ACTIVO
        // =====================================================

        private MesActividadHistoricaIA
            ObtenerPrimerMesConActividad(
                ResumenHistoricoIA resumen)
        {
            foreach (
                MesActividadHistoricaIA mes
                    in resumen.Meses)
            {
                if (mes.TurnosNoCancelados > 0)
                {
                    return mes;
                }
            }


            return null;
        }


        // =====================================================
        // ULTIMO MES ACTIVO
        // =====================================================

        private MesActividadHistoricaIA
            ObtenerUltimoMesConActividad(
                ResumenHistoricoIA resumen)
        {
            for (int i =
                    resumen.Meses.Count - 1;
                 i >= 0;
                 i--)
            {
                if (resumen
                    .Meses[i]
                    .TurnosNoCancelados > 0)
                {
                    return resumen.Meses[i];
                }
            }


            return null;
        }


        // =====================================================
        // MES MAS ACTIVO
        // =====================================================

        private string ResponderMesMasActivo(
            int cantidadMeses)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    cantidadMeses);


            if (resumen.TotalTurnos == 0)
            {
                return
                    "No tenés actividad registrada en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses.";
            }


            int maximo =
                0;


            foreach (
                MesActividadHistoricaIA mes
                    in resumen.Meses)
            {
                if (mes.TurnosNoCancelados >
                    maximo)
                {
                    maximo =
                        mes.TurnosNoCancelados;
                }
            }


            List<string> meses =
                new List<string>();


            foreach (
                MesActividadHistoricaIA mes
                    in resumen.Meses)
            {
                if (mes.TurnosNoCancelados ==
                        maximo
                    &&
                    maximo > 0)
                {
                    meses.Add(
                        mes.NombrePeriodo);
                }
            }


            if (meses.Count == 1)
            {
                return
                    "Tu mes con más actividad en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses fue "
                    +
                    meses[0]
                    +
                    ", con "
                    +
                    FormatearTurnosActivosHistorico(
                        maximo)
                    +
                    ".";
            }


            return
                "Hay un empate entre "
                +
                UnirListaHistorica(
                    meses)
                +
                ", con "
                +
                maximo
                +
                " turnos activos en cada mes.";
        }


        // =====================================================
        // RESUMEN HISTORICO
        // =====================================================

        private string ResponderResumenHistorico(
            int cantidadMeses)
        {
            AnalisisHistoricoNegocio negocio =
                new AnalisisHistoricoNegocio();


            ResumenHistoricoIA resumen =
                negocio.ObtenerUltimosMeses(
                    IdProfesionalActual,
                    cantidadMeses);


            if (resumen.TotalTurnos == 0)
            {
                return
                    "No tenés actividad registrada en los últimos "
                    +
                    cantidadMeses
                    +
                    " meses.";
            }


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "En los últimos "
                +
                cantidadMeses
                +
                " meses registraste "
                +
                resumen.TotalTurnos
                +
                (
                    resumen.TotalTurnos == 1
                        ? " turno."
                        : " turnos."
                ));


            if (resumen.Atendidos == 0)
            {
                texto.Append(
                    " Ninguno está marcado como atendido.");
            }
            else if (resumen.Atendidos == 1)
            {
                texto.Append(
                    " Uno terminó atendido.");
            }
            else
            {
                texto.Append(
                    " "
                    +
                    resumen.Atendidos
                    +
                    " terminaron atendidos.");
            }


            if (resumen.Cancelados == 0)
            {
                texto.Append(
                    " No hubo cancelaciones.");
            }
            else if (resumen.Cancelados == 1)
            {
                texto.Append(
                    " Hubo una cancelación.");
            }
            else
            {
                texto.Append(
                    " Hubo "
                    +
                    resumen.Cancelados
                    +
                    " cancelaciones.");
            }


            texto.AppendLine();

            texto.AppendLine();


            texto.Append(
                "Tu tasa de atención es del "
                +
                FormatearPorcentajeHistorico(
                    resumen.TasaAtencion)
                +
                " y la de cancelación del "
                +
                FormatearPorcentajeHistorico(
                    resumen.TasaCancelacion)
                +
                ".");


            if (resumen.MesMasActivo != null &&
                resumen.MesMasActivo
                    .TurnosNoCancelados > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "El mes con más actividad fue "
                    +
                    resumen.MesMasActivo.NombrePeriodo
                    +
                    ", con "
                    +
                    FormatearTurnosActivosHistorico(
                        resumen.MesMasActivo
                            .TurnosNoCancelados)
                    +
                    ".");
            }


            if (resumen.Atendidos > 0)
            {
                int maximoAtendidos =
                    0;


                foreach (
                    DiaSemanaHistoricoIA dia
                        in resumen.DiasSemana)
                {
                    if (dia.Atendidos >
                        maximoAtendidos)
                    {
                        maximoAtendidos =
                            dia.Atendidos;
                    }
                }


                List<string> dias =
                    new List<string>();


                foreach (
                    DiaSemanaHistoricoIA dia
                        in resumen.DiasSemana)
                {
                    if (dia.Atendidos ==
                            maximoAtendidos
                        &&
                        maximoAtendidos > 0)
                    {
                        dias.Add(
                            dia.NombreDia);
                    }
                }


                texto.AppendLine();

                texto.AppendLine();


                if (resumen.Atendidos < 3)
                {
                    if (dias.Count == 1)
                    {
                        texto.Append(
                            "Con los datos actuales, el "
                            +
                            dias[0]
                            +
                            " es el día con más pacientes "
                            +
                            "atendidos, aunque todavía hay "
                            +
                            "poco historial para considerarlo "
                            +
                            "un patrón.");
                    }
                    else
                    {
                        texto.Append(
                            "Todavía hay pocas atenciones "
                            +
                            "registradas para identificar "
                            +
                            "un día habitual.");
                    }
                }
                else
                {
                    if (dias.Count == 1)
                    {
                        texto.Append(
                            "El día de la semana con más "
                            +
                            "pacientes atendidos es el "
                            +
                            dias[0]
                            +
                            ".");
                    }
                    else
                    {
                        texto.Append(
                            "Los días con más pacientes "
                            +
                            "atendidos son "
                            +
                            UnirListaHistorica(
                                dias)
                            +
                            ".");
                    }
                }
            }


            return texto.ToString();
        }


        // =====================================================
        // ULTIMOS N MESES
        // =====================================================

        private int ObtenerCantidadMeses(
            string texto,
            int valorPredeterminado)
        {
            Match coincidencia =
                Regex.Match(
                    texto,
                    @"ultimos\s+(\d+)\s+meses");


            if (!coincidencia.Success)
            {
                return valorPredeterminado;
            }


            int cantidad;


            if (!int.TryParse(
                coincidencia.Groups[1].Value,
                out cantidad))
            {
                return valorPredeterminado;
            }


            if (cantidad < 1)
            {
                return valorPredeterminado;
            }


            if (cantidad > 60)
            {
                return 60;
            }


            return cantidad;
        }


        // =====================================================
        // FORMATO
        // =====================================================

        private string FormatearPorcentajeHistorico(
            decimal porcentaje)
        {
            return porcentaje.ToString(
                "0.0",
                new CultureInfo("es-AR"))
                +
                "%";
        }


        private string UnirListaHistorica(
            List<string> elementos)
        {
            if (elementos == null ||
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
