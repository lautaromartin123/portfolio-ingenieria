using BIZ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Consultorios
{
    public partial class PanelIAMedico
    {
        // =====================================================
        // PUNTO DE ENTRADA
        //
        // Orden:
        // 1. Fechas y días flexibles
        // 2. Agenda inmediata
        // 3. Histórico
        // 4. Asistente general
        // =====================================================

        protected void BtnPreguntarIAFlexible_Click(
            object sender,
            EventArgs e)
        {
            Page.MaintainScrollPositionOnPostBack =
                true;


            string entrada =
                txtPreguntaIA.Text.Trim();


            lblMensaje.Text =
                "";


            if (string.IsNullOrWhiteSpace(
                entrada))
            {
                lblMensaje.Text =
                    "Escribí una pregunta.";

                return;
            }


            if (entrada.Length > 2000)
            {
                lblMensaje.Text =
                    "El mensaje completo debe tener como máximo "
                    +
                    "2000 caracteres.";

                return;
            }


            try
            {
                AsegurarProfesional();


                if (IdProfesionalActual <= 0)
                {
                    return;
                }


                List<string> preguntas =
                    SepararPreguntasMultiples(
                        entrada);


                if (preguntas.Count == 0)
                {
                    lblMensaje.Text =
                        "No pude identificar ninguna pregunta.";

                    return;
                }


                if (preguntas.Count > 8)
                {
                    lblMensaje.Text =
                        "Podés enviar hasta 8 preguntas "
                        +
                        "en un mismo mensaje.";

                    return;
                }


                foreach (
                    string preguntaValidar
                        in preguntas)
                {
                    if (preguntaValidar.Length > 500)
                    {
                        lblMensaje.Text =
                            "Cada pregunta debe tener como máximo "
                            +
                            "500 caracteres.";

                        return;
                    }
                }


                foreach (
                    string pregunta
                        in preguntas)
                {
                    txtPreguntaIA.Text =
                        pregunta;


                    if (EsConsultaFechaFlexible(
                        pregunta))
                    {
                        string respuesta =
                            EjecutarConsultaFechaFlexible(
                                pregunta);


                        AgregarAlHistorial(
                            "Médico",
                            pregunta);


                        AgregarAlHistorial(
                            "IA",
                            respuesta);
                    }
                    else
                    {
                        BtnPreguntarIACompleto_Click(
                            sender,
                            e);
                    }
                }


                txtPreguntaIA.Text =
                    "";


                MostrarHistorialChat();


                MantenerVistaEnChat();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo consultar la agenda. "
                    +
                    Server.HtmlEncode(
                        ex.Message);


                MantenerVistaEnChat();
            }
        }


        // =====================================================
        // MULTIPLES PREGUNTAS EN UN SOLO ENVIO
        //
        // Separa por:
        // ?
        // salto de linea
        // ;
        //
        // Ejemplo:
        // "¿Qué turnos tengo esta semana?
        //  ¿Y la semana que viene?
        //  ¿Qué día, hora y paciente tengo ese turno?"
        // =====================================================

        private List<string> SepararPreguntasMultiples(
            string entrada)
        {
            List<string> resultado =
                new List<string>();


            if (string.IsNullOrWhiteSpace(
                entrada))
            {
                return resultado;
            }


            string[] partes =
                Regex.Split(
                    entrada,
                    @"(?:\?+|[\r\n]+|;+)");


            foreach (
                string parteOriginal
                    in partes)
            {
                string parte =
                    (
                        parteOriginal
                        ??
                        ""
                    ).Trim();


                parte =
                    parte.TrimStart(
                        '¿',
                        ' ',
                        '\t',
                        '-');


                if (parte.StartsWith(
                    "y ",
                    StringComparison.OrdinalIgnoreCase))
                {
                    parte =
                        parte.Substring(2)
                            .Trim();
                }


                if (!string.IsNullOrWhiteSpace(
                    parte))
                {
                    resultado.Add(
                        parte);
                }
            }


            if (resultado.Count == 0)
            {
                resultado.Add(
                    entrada.Trim());
            }


            return resultado;
        }


        // =====================================================
        // MANTENER LA VISTA EN EL CHAT DESPUES DEL POSTBACK
        // =====================================================

        private void MantenerVistaEnChat()
        {
            string idCampo =
                txtPreguntaIA.ClientID;


            string script =
                "setTimeout(function(){"
                +
                "var campo=document.getElementById('"
                +
                idCampo
                +
                "');"
                +
                "if(campo){"
                +
                "campo.scrollIntoView({behavior:'auto',block:'center'});"
                +
                "campo.focus();"
                +
                "}else{"
                +
                "window.scrollTo(0,document.body.scrollHeight);"
                +
                "}"
                +
                "},0);";


            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "MantenerVistaEnChat",
                script,
                true);
        }


        // =====================================================
        // DETECTAR CONSULTA FLEXIBLE
        // =====================================================

        private bool EsConsultaFechaFlexible(
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


            // =================================================
            // CONSULTAS OPERATIVAS
            // =================================================

            if (
                texto.Contains(
                    "trabajo pero no tengo")
                ||
                texto.Contains(
                    "trabajo y no tengo")
                ||
                texto.Contains(
                    "dias sin turnos")
                ||
                texto.Contains(
                    "dias sin pacientes")
                ||
                texto.Contains(
                    "trabajo sin turnos"))
            {
                return false;
            }


            // =================================================
            // SEGUIMIENTO DEL ULTIMO PERIODO
            // =================================================

            if (EsSeguimientoUltimoPeriodo(
                texto))
            {
                return true;
            }


            // =================================================
            // ESTA SEMANA
            // =================================================

            if (texto.Contains(
                "esta semana"))
            {
                return true;
            }


            // =================================================
            // PROXIMA SEMANA / SEMANA QUE VIENE
            // =================================================

            if (
                texto.Contains(
                    "proxima semana")
                ||
                texto.Contains(
                    "semana que viene"))
            {
                return true;
            }


            // =================================================
            // SEMANAS RELATIVAS
            // =================================================

            int cantidadSemanas;


            if (IntentarResolverSemanasRelativas(
                texto,
                out cantidadSemanas))
            {
                return true;
            }


            // =================================================
            // FECHA NUMERICA
            // =================================================

            if (Regex.IsMatch(
                texto,
                @"\b\d{1,2}/\d{1,2}(/\d{2,4})?\b"))
            {
                return true;
            }


            // =================================================
            // FECHA ESCRITA
            // =================================================

            if (Regex.IsMatch(
                texto,
                @"\b\d{1,2}\s+de\s+(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)\b"))
            {
                return true;
            }


            // =================================================
            // RANGO ENTRE DIAS
            // =================================================

            if (texto.Contains(
                    "entre")
                &&
                texto.Contains(
                    " y ")
                &&
                ContieneDiaSemana(
                    texto))
            {
                return true;
            }


            // =================================================
            // CONSULTA POR DIA DE SEMANA
            // =================================================

            if (ContieneDiaSemana(
                    texto)
                &&
                (
                    texto.Contains(
                        "que tengo")
                    ||
                    texto.Contains(
                        "turnos")
                    ||
                    texto.Contains(
                        "pacientes")
                    ||
                    texto.Contains(
                        "agenda")
                    ||
                    texto.Contains(
                        "a que hora empiezo")
                    ||
                    texto.Contains(
                        "a que hora comienzo")
                    ||
                    texto.Contains(
                        "a que hora termino")
                    ||
                    texto.Contains(
                        "primer turno")
                    ||
                    texto.Contains(
                        "ultimo turno")
                    ||
                    texto.Contains(
                        "tengo algo")
                ))
            {
                return true;
            }


            return false;
        }


        // =====================================================
        // ROUTER
        // =====================================================

        private string EjecutarConsultaFechaFlexible(
            string pregunta)
        {
            string texto =
                NormalizarTexto(
                    pregunta);


            // =================================================
            // SEGUIMIENTO CONVERSACIONAL
            // =================================================

            if (EsSeguimientoUltimoPeriodo(
                texto))
            {
                return ResponderDetalleUltimoPeriodo();
            }


            // =================================================
            // ESTA SEMANA
            // =================================================

            if (texto.Contains(
                "esta semana"))
            {
                DateTime lunes =
                    ObtenerLunesSemana(
                        DateTime.Today);


                DateTime hasta =
                    lunes.AddDays(7);


                GuardarUltimoPeriodo(
                    lunes,
                    hasta);


                return ResponderPeriodoAgenda(
                    "Esta semana",
                    lunes,
                    hasta);
            }


            // =================================================
            // PROXIMA SEMANA / SEMANA QUE VIENE
            // =================================================

            if (
                texto.Contains(
                    "proxima semana")
                ||
                texto.Contains(
                    "semana que viene"))
            {
                DateTime lunes =
                    ObtenerLunesSemana(
                        DateTime.Today)
                    .AddDays(7);


                DateTime hasta =
                    lunes.AddDays(7);


                GuardarUltimoPeriodo(
                    lunes,
                    hasta);


                return ResponderPeriodoAgenda(
                    "La próxima semana",
                    lunes,
                    hasta);
            }


            // =================================================
            // SEMANAS RELATIVAS
            // =================================================

            int cantidadSemanas;


            if (IntentarResolverSemanasRelativas(
                texto,
                out cantidadSemanas))
            {
                DateTime referencia =
                    DateTime.Today.AddDays(
                        cantidadSemanas * 7);


                DateTime lunes =
                    ObtenerLunesSemana(
                        referencia);


                DateTime hasta =
                    lunes.AddDays(7);


                GuardarUltimoPeriodo(
                    lunes,
                    hasta);


                string titulo;


                if (cantidadSemanas == 1)
                {
                    titulo =
                        "Dentro de una semana";
                }
                else
                {
                    titulo =
                        "Dentro de "
                        +
                        cantidadSemanas
                        +
                        " semanas";
                }


                return ResponderPeriodoAgenda(
                    titulo,
                    lunes,
                    hasta);
            }


            // =================================================
            // RANGO ENTRE DOS DIAS
            // =================================================

            if (texto.Contains(
                    "entre")
                &&
                ContieneDiaSemana(
                    texto))
            {
                DateTime desdeRango;
                DateTime hastaRango;


                if (IntentarResolverRangoDias(
                    texto,
                    out desdeRango,
                    out hastaRango))
                {
                    GuardarUltimoPeriodo(
                        desdeRango.Date,
                        hastaRango.Date.AddDays(1));


                    return ResponderRangoFlexible(
                        pregunta,
                        desdeRango,
                        hastaRango);
                }
            }


            // =================================================
            // FECHA O DIA CONCRETO
            // =================================================

            DateTime fecha;


            if (!IntentarResolverFechaFlexible(
                texto,
                out fecha))
            {
                return
                    "No pude identificar la fecha "
                    +
                    "que querés consultar.";
            }


            GuardarUltimoPeriodo(
                fecha.Date,
                fecha.Date.AddDays(1));


            return ResponderFechaConcreta(
                pregunta,
                fecha);
        }


        // =====================================================
        // RESPONDER FECHA CONCRETA
        // =====================================================

        private string ResponderFechaConcreta(
            string pregunta,
            DateTime fecha)
        {
            string texto =
                NormalizarTexto(
                    pregunta);


            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    fecha.Date,
                    fecha.Date.AddDays(1));


            string nombreFecha =
                FormatearFechaFlexible(
                    fecha);


            // =================================================
            // TENGO ALGO
            // =================================================

            if (texto.Contains(
                "tengo algo"))
            {
                if (turnos.Count == 0)
                {
                    return
                        "No. No tenés turnos agendados el "
                        +
                        nombreFecha
                        +
                        ".";
                }


                return
                    "Sí. El "
                    +
                    nombreFecha
                    +
                    " tenés "
                    +
                    turnos.Count
                    +
                    (
                        turnos.Count == 1
                            ? " turno."
                            : " turnos."
                    );
            }


            // =================================================
            // CUANTOS TURNOS
            // =================================================

            if (texto.Contains(
                    "cuantos turnos"))
            {
                if (turnos.Count == 0)
                {
                    return
                        "No tenés turnos agendados el "
                        +
                        nombreFecha
                        +
                        ".";
                }


                return
                    "El "
                    +
                    nombreFecha
                    +
                    " tenés "
                    +
                    turnos.Count
                    +
                    (
                        turnos.Count == 1
                            ? " turno."
                            : " turnos."
                    );
            }


            // =================================================
            // PRIMER TURNO
            // =================================================

            if (texto.Contains(
                    "a que hora empiezo")
                ||
                texto.Contains(
                    "a que hora comienzo")
                ||
                texto.Contains(
                    "primer turno"))
            {
                if (turnos.Count == 0)
                {
                    return
                        "No tenés turnos agendados el "
                        +
                        nombreFecha
                        +
                        ".";
                }


                return
                    "El "
                    +
                    nombreFecha
                    +
                    " tu primer turno es a las "
                    +
                    turnos[0]
                        .FechaHora
                        .ToString("HH:mm")
                    +
                    ".";
            }


            // =================================================
            // HORA DE FIN
            // =================================================

            if (texto.Contains(
                    "a que hora termino"))
            {
                if (turnos.Count == 0)
                {
                    return
                        "No tenés turnos agendados el "
                        +
                        nombreFecha
                        +
                        ".";
                }


                TurnoAgendaIA ultimo =
                    turnos[
                        turnos.Count - 1];


                return
                    "El "
                    +
                    nombreFecha
                    +
                    " terminás tu último turno "
                    +
                    "aproximadamente a las "
                    +
                    ultimo.FechaFin
                        .ToString("HH:mm")
                    +
                    ".";
            }


            // =================================================
            // ULTIMO TURNO
            // =================================================

            if (texto.Contains(
                "ultimo turno"))
            {
                if (turnos.Count == 0)
                {
                    return
                        "No tenés turnos agendados el "
                        +
                        nombreFecha
                        +
                        ".";
                }


                TurnoAgendaIA ultimo =
                    turnos[
                        turnos.Count - 1];


                return
                    "El último turno del "
                    +
                    nombreFecha
                    +
                    " empieza a las "
                    +
                    ultimo.FechaHora
                        .ToString("HH:mm")
                    +
                    ".";
            }


            // =================================================
            // AGENDA COMPLETA
            // =================================================

            if (turnos.Count == 0)
            {
                return
                    "No tenés turnos agendados el "
                    +
                    nombreFecha
                    +
                    ".";
            }


            StringBuilder respuesta =
                new StringBuilder();


            respuesta.Append(
                "El "
                +
                nombreFecha
                +
                " tenés "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno."
                        : " turnos."
                ));


            respuesta.AppendLine();

            respuesta.AppendLine();


            List<string> horarios =
                new List<string>();


            foreach (
                TurnoAgendaIA turno
                    in turnos)
            {
                horarios.Add(
                    turno.FechaHora
                        .ToString("HH:mm"));
            }


            respuesta.Append(
                "Horarios: "
                +
                UnirListaFlexible(
                    horarios)
                +
                ".");


            if (turnos.Count > 1)
            {
                respuesta.AppendLine();

                respuesta.AppendLine();


                respuesta.Append(
                    "El primero es a las "
                    +
                    turnos[0]
                        .FechaHora
                        .ToString("HH:mm")
                    +
                    " y el último empieza a las "
                    +
                    turnos[
                        turnos.Count - 1]
                        .FechaHora
                        .ToString("HH:mm")
                    +
                    ".");
            }


            return respuesta.ToString();
        }


        // =====================================================
        // AGENDA PROXIMA SEMANA
        // =====================================================

        private string ResponderAgendaProximaSemana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime lunes =
                ObtenerLunesSemana(
                    DateTime.Today)
                .AddDays(7);


            DateTime lunesSiguiente =
                lunes.AddDays(7);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    lunes,
                    lunesSiguiente);


            if (turnos.Count == 0)
            {
                return
                    "La próxima semana no tenés turnos "
                    +
                    "agendados todavía.";
            }


            Dictionary<DateTime, List<TurnoAgendaIA>>
                porDia =
                    AgruparTurnosPorDia(
                        turnos);


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "La próxima semana tenés "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno"
                        : " turnos"
                )
                +
                " distribuidos en "
                +
                porDia.Count
                +
                (
                    porDia.Count == 1
                        ? " día."
                        : " días."
                ));


            List<DateTime> fechas =
                ObtenerFechasOrdenadas(
                    porDia);


            foreach (
                DateTime fecha
                    in fechas)
            {
                List<TurnoAgendaIA> turnosDia =
                    porDia[fecha];


                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    FormatearFechaFlexible(
                        fecha)
                    +
                    ": "
                    +
                    turnosDia.Count
                    +
                    (
                        turnosDia.Count == 1
                            ? " turno"
                            : " turnos"
                    ));


                texto.Append(
                    ", de "
                    +
                    turnosDia[0]
                        .FechaHora
                        .ToString("HH:mm")
                    +
                    " a "
                    +
                    turnosDia[
                        turnosDia.Count - 1]
                        .FechaFin
                        .ToString("HH:mm")
                    +
                    ".");
            }


            return texto.ToString();
        }


        // =====================================================
        // DIAS CON TURNOS PROXIMA SEMANA
        // =====================================================

        private string ResponderDiasConTurnosProximaSemana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime lunes =
                ObtenerLunesSemana(
                    DateTime.Today)
                .AddDays(7);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    lunes,
                    lunes.AddDays(7));


            if (turnos.Count == 0)
            {
                return
                    "La próxima semana no tenés turnos "
                    +
                    "agendados todavía.";
            }


            Dictionary<DateTime, List<TurnoAgendaIA>>
                porDia =
                    AgruparTurnosPorDia(
                        turnos);


            List<DateTime> fechas =
                ObtenerFechasOrdenadas(
                    porDia);


            List<string> dias =
                new List<string>();


            foreach (
                DateTime fecha
                    in fechas)
            {
                dias.Add(
                    FormatearFechaFlexible(
                        fecha)
                    +
                    " ("
                    +
                    porDia[fecha].Count
                    +
                    (
                        porDia[fecha].Count == 1
                            ? " turno)"
                            : " turnos)"
                    ));
            }


            return
                "La próxima semana tenés turnos el "
                +
                UnirListaFlexible(
                    dias)
                +
                ".";
        }


        // =====================================================
        // RANGO FLEXIBLE
        // =====================================================

        private string ResponderRangoFlexible(
            string pregunta,
            DateTime desde,
            DateTime hasta)
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    desde.Date,
                    hasta.Date.AddDays(1));


            string textoPregunta =
                NormalizarTexto(
                    pregunta);


            if (turnos.Count == 0)
            {
                return
                    "No tenés turnos entre el "
                    +
                    FormatearFechaFlexible(
                        desde)
                    +
                    " y el "
                    +
                    FormatearFechaFlexible(
                        hasta)
                    +
                    ".";
            }


            Dictionary<DateTime, List<TurnoAgendaIA>>
                porDia =
                    AgruparTurnosPorDia(
                        turnos);


            // =================================================
            // DIA MAS OCUPADO
            // =================================================

            if (textoPregunta.Contains(
                    "mas ocupado")
                ||
                textoPregunta.Contains(
                    "mas carga")
                ||
                textoPregunta.Contains(
                    "mas turnos"))
            {
                int maximo =
                    0;


                foreach (
                    KeyValuePair<DateTime, List<TurnoAgendaIA>>
                        item
                        in porDia)
                {
                    if (item.Value.Count >
                        maximo)
                    {
                        maximo =
                            item.Value.Count;
                    }
                }


                List<string> dias =
                    new List<string>();


                foreach (
                    KeyValuePair<DateTime, List<TurnoAgendaIA>>
                        item
                        in porDia)
                {
                    if (item.Value.Count ==
                        maximo)
                    {
                        dias.Add(
                            FormatearFechaFlexible(
                                item.Key));
                    }
                }


                if (dias.Count == 1)
                {
                    return
                        "Entre el "
                        +
                        FormatearFechaFlexible(
                            desde)
                        +
                        " y el "
                        +
                        FormatearFechaFlexible(
                            hasta)
                        +
                        ", el día con más carga es el "
                        +
                        dias[0]
                        +
                        ", con "
                        +
                        maximo
                        +
                        (
                            maximo == 1
                                ? " turno."
                                : " turnos."
                        );
                }


                return
                    "La mayor carga está empatada entre "
                    +
                    UnirListaFlexible(
                        dias)
                    +
                    ", con "
                    +
                    maximo
                    +
                    " turnos en cada día.";
            }


            return
                "Entre el "
                +
                FormatearFechaFlexible(
                    desde)
                +
                " y el "
                +
                FormatearFechaFlexible(
                    hasta)
                +
                " tenés "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno"
                        : " turnos"
                )
                +
                " distribuidos en "
                +
                porDia.Count
                +
                (
                    porDia.Count == 1
                        ? " día."
                        : " días."
                );
        }



        // =====================================================
        // SEMANAS RELATIVAS
        //
        // Ejemplos soportados:
        // dentro de 2 semanas
        // dentro de dos semanas
        // en 3 semanas
        // de aca a 4 semanas
        // =====================================================

        private bool IntentarResolverSemanasRelativas(
            string texto,
            out int cantidadSemanas)
        {
            cantidadSemanas = 0;


            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return false;
            }


            Match coincidencia =
                Regex.Match(
                    texto,
                    @"\b(?:dentro\s+de|en|de\s+aca\s+a)\s+"
                    +
                    @"(\d{1,2}|un|una|dos|tres|cuatro|cinco|seis|siete|ocho|nueve|diez)"
                    +
                    @"\s+semanas?\b");


            if (!coincidencia.Success)
            {
                return false;
            }


            string valor =
                coincidencia.Groups[1]
                    .Value
                    .Trim()
                    .ToLowerInvariant();


            int numero;


            if (int.TryParse(
                valor,
                out numero))
            {
                if (numero <= 0 ||
                    numero > 52)
                {
                    return false;
                }


                cantidadSemanas =
                    numero;


                return true;
            }


            switch (valor)
            {
                case "un":
                case "una":
                    cantidadSemanas = 1;
                    return true;

                case "dos":
                    cantidadSemanas = 2;
                    return true;

                case "tres":
                    cantidadSemanas = 3;
                    return true;

                case "cuatro":
                    cantidadSemanas = 4;
                    return true;

                case "cinco":
                    cantidadSemanas = 5;
                    return true;

                case "seis":
                    cantidadSemanas = 6;
                    return true;

                case "siete":
                    cantidadSemanas = 7;
                    return true;

                case "ocho":
                    cantidadSemanas = 8;
                    return true;

                case "nueve":
                    cantidadSemanas = 9;
                    return true;

                case "diez":
                    cantidadSemanas = 10;
                    return true;

                default:
                    return false;
            }
        }


        // =====================================================
        // RESPONDER SEMANA RELATIVA
        // =====================================================

        private string ResponderSemanaRelativa(
            int cantidadSemanas)
        {
            if (cantidadSemanas <= 0)
            {
                return
                    "No pude identificar cuántas semanas "
                    +
                    "querés consultar.";
            }


            DateTime fechaReferencia =
                DateTime.Today
                    .AddDays(
                        cantidadSemanas * 7);


            DateTime lunes =
                ObtenerLunesSemana(
                    fechaReferencia);


            DateTime lunesSiguiente =
                lunes.AddDays(7);


            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    lunes,
                    lunesSiguiente);


            DateTime domingo =
                lunes.AddDays(6);


            string expresionSemanas =
                cantidadSemanas == 1
                    ? "Dentro de una semana"
                    : "Dentro de "
                      + cantidadSemanas
                      + " semanas";


            if (turnos == null ||
                turnos.Count == 0)
            {
                return
                    expresionSemanas
                    +
                    ", en la semana del "
                    +
                    FormatearFechaFlexible(
                        lunes)
                    +
                    " al "
                    +
                    FormatearFechaFlexible(
                        domingo)
                    +
                    ", no tenés turnos agendados.";
            }


            Dictionary<DateTime, List<TurnoAgendaIA>>
                porDia =
                    AgruparTurnosPorDia(
                        turnos);


            List<DateTime> fechas =
                ObtenerFechasOrdenadas(
                    porDia);


            StringBuilder respuesta =
                new StringBuilder();


            respuesta.Append(
                expresionSemanas
                +
                ", en la semana del "
                +
                FormatearFechaFlexible(
                    lunes)
                +
                " al "
                +
                FormatearFechaFlexible(
                    domingo)
                +
                ", tenés "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno"
                        : " turnos"
                )
                +
                " distribuidos en "
                +
                porDia.Count
                +
                (
                    porDia.Count == 1
                        ? " día."
                        : " días."
                ));


            foreach (
                DateTime fecha
                    in fechas)
            {
                List<TurnoAgendaIA> turnosDia =
                    porDia[fecha];


                respuesta.AppendLine();

                respuesta.AppendLine();


                respuesta.Append(
                    FormatearFechaFlexible(
                        fecha)
                    +
                    ": "
                    +
                    turnosDia.Count
                    +
                    (
                        turnosDia.Count == 1
                            ? " turno"
                            : " turnos"
                    )
                    +
                    ".");


                List<string> horarios =
                    new List<string>();


                foreach (
                    TurnoAgendaIA turno
                        in turnosDia)
                {
                    horarios.Add(
                        turno.FechaHora
                            .ToString(
                                "HH:mm"));
                }


                respuesta.Append(
                    " Horarios: "
                    +
                    UnirListaFlexible(
                        horarios)
                    +
                    ".");
            }


            return respuesta.ToString();
        }





        // =====================================================
        // CONTEXTO CONVERSACIONAL DEL ULTIMO PERIODO
        // =====================================================

        private void GuardarUltimoPeriodo(
            DateTime desde,
            DateTime hastaExclusivo)
        {
            Session[
                "IA_MEDICO_ULTIMO_DESDE"] =
                desde;


            Session[
                "IA_MEDICO_ULTIMO_HASTA"] =
                hastaExclusivo;
        }


        private bool HayUltimoPeriodo()
        {
            return
                Session[
                    "IA_MEDICO_ULTIMO_DESDE"]
                    != null
                &&
                Session[
                    "IA_MEDICO_ULTIMO_HASTA"]
                    != null;
        }


        private bool EsSeguimientoUltimoPeriodo(
            string texto)
        {
            if (!HayUltimoPeriodo())
            {
                return false;
            }


            return
                texto.Contains(
                    "ese turno")
                ||
                texto.Contains(
                    "ese paciente")
                ||
                texto.Contains(
                    "ese horario")
                ||
                texto.Contains(
                    "esos turnos")
                ||
                texto.Contains(
                    "esos pacientes")
                ||
                texto.Contains(
                    "que dia hora")
                ||
                texto.Contains(
                    "que dia y hora")
                ||
                texto.Contains(
                    "dia hora y paciente")
                ||
                texto.Contains(
                    "hora y paciente")
                ||
                texto.Contains(
                    "quien es el paciente")
                ||
                texto.Contains(
                    "cual es el paciente")
                ||
                texto.Contains(
                    "detalle del turno")
                ||
                texto.Contains(
                    "detalles del turno");
        }


        private string ResponderPeriodoAgenda(
            string titulo,
            DateTime desde,
            DateTime hastaExclusivo)
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    desde,
                    hastaExclusivo);


            if (turnos == null ||
                turnos.Count == 0)
            {
                return
                    titulo
                    +
                    " no tenés turnos agendados.";
            }


            Dictionary<DateTime, List<TurnoAgendaIA>>
                porDia =
                    AgruparTurnosPorDia(
                        turnos);


            List<DateTime> fechas =
                ObtenerFechasOrdenadas(
                    porDia);


            StringBuilder respuesta =
                new StringBuilder();


            respuesta.Append(
                titulo
                +
                " tenés "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno"
                        : " turnos"
                )
                +
                " distribuidos en "
                +
                porDia.Count
                +
                (
                    porDia.Count == 1
                        ? " día."
                        : " días."
                ));


            foreach (
                DateTime fecha
                    in fechas)
            {
                List<TurnoAgendaIA> turnosDia =
                    porDia[fecha];


                respuesta.AppendLine();

                respuesta.AppendLine();


                respuesta.Append(
                    FormatearFechaFlexible(
                        fecha)
                    +
                    ": "
                    +
                    turnosDia.Count
                    +
                    (
                        turnosDia.Count == 1
                            ? " turno"
                            : " turnos"
                    )
                    +
                    ". Horarios: ");


                List<string> horarios =
                    new List<string>();


                foreach (
                    TurnoAgendaIA turno
                        in turnosDia)
                {
                    horarios.Add(
                        turno.FechaHora
                            .ToString(
                                "HH:mm"));
                }


                respuesta.Append(
                    UnirListaFlexible(
                        horarios));


                respuesta.Append(".");
            }


            return respuesta.ToString();
        }


        private string ResponderDetalleUltimoPeriodo()
        {
            if (!HayUltimoPeriodo())
            {
                return
                    "Primero consultá un período de la agenda.";
            }


            DateTime desde =
                (DateTime)Session[
                    "IA_MEDICO_ULTIMO_DESDE"];


            DateTime hasta =
                (DateTime)Session[
                    "IA_MEDICO_ULTIMO_HASTA"];


            string cadenaConexion =
                ConfigurationManager
                    .ConnectionStrings[
                        "Conexion"]
                    .ConnectionString;


            string sql = @"
                SELECT
                    T.IdTurno,
                    T.FechaHora,
                    T.DuracionMinutos,
                    P.Nombre AS PacienteNombre,
                    P.Apellido AS PacienteApellido,
                    ET.Descripcion AS EstadoTurno

                FROM dbo.Turno T

                INNER JOIN dbo.Paciente P
                    ON P.IdPaciente = T.IdPaciente

                INNER JOIN dbo.EstadoTurno ET
                    ON ET.IdEstadoTurno = T.IdEstadoTurno

                WHERE T.IdProfesional = @IdProfesional
                  AND T.FechaHora >= @Desde
                  AND T.FechaHora < @Hasta
                  AND ET.Descripcion <> 'Cancelado'

                ORDER BY T.FechaHora";


            using (SqlConnection conexion =
                new SqlConnection(
                    cadenaConexion))
            {
                using (SqlCommand comando =
                    new SqlCommand(
                        sql,
                        conexion))
                {
                    comando.Parameters.Add(
                        "@IdProfesional",
                        SqlDbType.Int
                    ).Value =
                        IdProfesionalActual;


                    comando.Parameters.Add(
                        "@Desde",
                        SqlDbType.DateTime
                    ).Value =
                        desde;


                    comando.Parameters.Add(
                        "@Hasta",
                        SqlDbType.DateTime
                    ).Value =
                        hasta;


                    conexion.Open();


                    using (SqlDataReader reader =
                        comando.ExecuteReader())
                    {
                        List<string> detalles =
                            new List<string>();


                        while (reader.Read())
                        {
                            DateTime fechaHora =
                                Convert.ToDateTime(
                                    reader[
                                        "FechaHora"]);


                            string nombre =
                                reader[
                                    "PacienteNombre"]
                                    .ToString();


                            string apellido =
                                reader[
                                    "PacienteApellido"]
                                    .ToString();


                            string paciente =
                                (
                                    nombre
                                    +
                                    " "
                                    +
                                    apellido
                                ).Trim();


                            string estado =
                                reader[
                                    "EstadoTurno"]
                                    .ToString();


                            string detalle =
                                fechaHora.ToString(
                                    "dddd d 'de' MMMM 'de' yyyy",
                                    new CultureInfo(
                                        "es-AR"))
                                +
                                " a las "
                                +
                                fechaHora.ToString(
                                    "HH:mm")
                                +
                                " con "
                                +
                                paciente
                                +
                                " ("
                                +
                                estado
                                +
                                ")";


                            detalles.Add(
                                detalle);
                        }


                        if (detalles.Count == 0)
                        {
                            return
                                "No hay turnos activos "
                                +
                                "en el último período consultado.";
                        }


                        if (detalles.Count == 1)
                        {
                            return
                                "Ese turno es el "
                                +
                                detalles[0]
                                +
                                ".";
                        }


                        StringBuilder respuesta =
                            new StringBuilder();


                        respuesta.Append(
                            "En ese período tenés "
                            +
                            detalles.Count
                            +
                            " turnos:");


                        for (int i = 0;
                             i < detalles.Count;
                             i++)
                        {
                            respuesta.AppendLine();


                            respuesta.Append(
                                (i + 1)
                                +
                                ". "
                                +
                                detalles[i]
                                +
                                ".");
                        }


                        return respuesta.ToString();
                    }
                }
            }
        }


        // =====================================================
        // RESOLVER FECHA FLEXIBLE
        // =====================================================

        private bool IntentarResolverFechaFlexible(
            string texto,
            out DateTime fecha)
        {
            fecha =
                DateTime.MinValue;


            Match numerica =
                Regex.Match(
                    texto,
                    @"\b(\d{1,2})/(\d{1,2})(?:/(\d{2,4}))?\b");


            if (numerica.Success)
            {
                int dia;
                int mes;
                int anio =
                    DateTime.Today.Year;


                if (!int.TryParse(
                    numerica.Groups[1].Value,
                    out dia))
                {
                    return false;
                }


                if (!int.TryParse(
                    numerica.Groups[2].Value,
                    out mes))
                {
                    return false;
                }


                if (numerica.Groups[3].Success)
                {
                    if (!int.TryParse(
                        numerica.Groups[3].Value,
                        out anio))
                    {
                        return false;
                    }


                    if (anio < 100)
                    {
                        anio +=
                            2000;
                    }
                }


                return IntentarCrearFecha(
                    anio,
                    mes,
                    dia,
                    out fecha);
            }


            Match escrita =
                Regex.Match(
                    texto,
                    @"\b(\d{1,2})\s+de\s+(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)(?:\s+de\s+(\d{4}))?\b");


            if (escrita.Success)
            {
                int dia;


                if (!int.TryParse(
                    escrita.Groups[1].Value,
                    out dia))
                {
                    return false;
                }


                int mes =
                    ObtenerNumeroMes(
                        escrita.Groups[2].Value);


                int anio =
                    DateTime.Today.Year;


                if (escrita.Groups[3].Success)
                {
                    if (!int.TryParse(
                        escrita.Groups[3].Value,
                        out anio))
                    {
                        return false;
                    }
                }


                return IntentarCrearFecha(
                    anio,
                    mes,
                    dia,
                    out fecha);
            }


            int numeroDia =
                ObtenerDiaSemanaDelTexto(
                    texto);


            if (numeroDia <= 0)
            {
                return false;
            }


            bool siguienteEstricto =
                texto.Contains("proximo lunes")
                ||
                texto.Contains("proximo martes")
                ||
                texto.Contains("proximo miercoles")
                ||
                texto.Contains("proximo jueves")
                ||
                texto.Contains("proximo viernes")
                ||
                texto.Contains("proximo sabado")
                ||
                texto.Contains("proximo domingo");


            fecha =
                ObtenerProximaFechaDiaSemana(
                    numeroDia,
                    siguienteEstricto);


            return true;
        }


        // =====================================================
        // RANGO ENTRE LUNES Y VIERNES
        // =====================================================

        private bool IntentarResolverRangoDias(
            string texto,
            out DateTime desde,
            out DateTime hasta)
        {
            desde =
                DateTime.MinValue;


            hasta =
                DateTime.MinValue;


            Match rango =
                Regex.Match(
                    texto,
                    @"entre\s+(?:el\s+)?(lunes|martes|miercoles|jueves|viernes|sabado|domingo)\s+y\s+(?:el\s+)?(lunes|martes|miercoles|jueves|viernes|sabado|domingo)");


            if (!rango.Success)
            {
                return false;
            }


            int diaDesde =
                ObtenerNumeroDiaSemanaNombre(
                    rango.Groups[1].Value);


            int diaHasta =
                ObtenerNumeroDiaSemanaNombre(
                    rango.Groups[2].Value);


            if (diaDesde <= 0 ||
                diaHasta <= 0)
            {
                return false;
            }


            desde =
                ObtenerProximaFechaDiaSemana(
                    diaDesde,
                    false);


            int diferencia =
                (
                    diaHasta
                    -
                    diaDesde
                    +
                    7
                )
                %
                7;


            hasta =
                desde.AddDays(
                    diferencia);


            return true;
        }


        // =====================================================
        // PROXIMA FECHA DE UN DIA
        // =====================================================

        private DateTime ObtenerProximaFechaDiaSemana(
            int diaSemana,
            bool siguienteEstricto)
        {
            DateTime inicio =
                DateTime.Today;


            if (siguienteEstricto)
            {
                inicio =
                    inicio.AddDays(1);
            }


            DateTime fecha =
                inicio;


            for (int i = 0;
                 i < 8;
                 i++)
            {
                if (ConvertirDiaSemanaFlexible(
                    fecha.DayOfWeek)
                    ==
                    diaSemana)
                {
                    return fecha.Date;
                }


                fecha =
                    fecha.AddDays(1);
            }


            return inicio.Date;
        }


        // =====================================================
        // CONTIENE DIA
        // =====================================================

        private bool ContieneDiaSemana(
            string texto)
        {
            return
                texto.Contains("lunes")
                ||
                texto.Contains("martes")
                ||
                texto.Contains("miercoles")
                ||
                texto.Contains("jueves")
                ||
                texto.Contains("viernes")
                ||
                texto.Contains("sabado")
                ||
                texto.Contains("domingo");
        }


        private int ObtenerDiaSemanaDelTexto(
            string texto)
        {
            if (texto.Contains("lunes"))
                return 1;

            if (texto.Contains("martes"))
                return 2;

            if (texto.Contains("miercoles"))
                return 3;

            if (texto.Contains("jueves"))
                return 4;

            if (texto.Contains("viernes"))
                return 5;

            if (texto.Contains("sabado"))
                return 6;

            if (texto.Contains("domingo"))
                return 7;


            return 0;
        }


        private int ObtenerNumeroDiaSemanaNombre(
            string nombre)
        {
            switch (nombre)
            {
                case "lunes":
                    return 1;

                case "martes":
                    return 2;

                case "miercoles":
                    return 3;

                case "jueves":
                    return 4;

                case "viernes":
                    return 5;

                case "sabado":
                    return 6;

                case "domingo":
                    return 7;

                default:
                    return 0;
            }
        }


        private int ConvertirDiaSemanaFlexible(
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
        // MESES
        // =====================================================

        private int ObtenerNumeroMes(
            string mes)
        {
            switch (mes)
            {
                case "enero":
                    return 1;

                case "febrero":
                    return 2;

                case "marzo":
                    return 3;

                case "abril":
                    return 4;

                case "mayo":
                    return 5;

                case "junio":
                    return 6;

                case "julio":
                    return 7;

                case "agosto":
                    return 8;

                case "septiembre":
                    return 9;

                case "octubre":
                    return 10;

                case "noviembre":
                    return 11;

                case "diciembre":
                    return 12;

                default:
                    return 0;
            }
        }


        private bool IntentarCrearFecha(
            int anio,
            int mes,
            int dia,
            out DateTime fecha)
        {
            fecha =
                DateTime.MinValue;


            try
            {
                fecha =
                    new DateTime(
                        anio,
                        mes,
                        dia);


                return true;
            }
            catch
            {
                return false;
            }
        }


        // =====================================================
        // AGRUPAR TURNOS
        // =====================================================

        private Dictionary<DateTime, List<TurnoAgendaIA>>
            AgruparTurnosPorDia(
                List<TurnoAgendaIA> turnos)
        {
            Dictionary<DateTime, List<TurnoAgendaIA>>
                resultado =
                    new Dictionary<DateTime, List<TurnoAgendaIA>>();


            foreach (
                TurnoAgendaIA turno
                    in turnos)
            {
                DateTime fecha =
                    turno.FechaHora.Date;


                if (!resultado.ContainsKey(
                    fecha))
                {
                    resultado.Add(
                        fecha,
                        new List<TurnoAgendaIA>());
                }


                resultado[fecha].Add(
                    turno);
            }


            return resultado;
        }


        private List<DateTime> ObtenerFechasOrdenadas(
            Dictionary<DateTime, List<TurnoAgendaIA>> datos)
        {
            List<DateTime> fechas =
                new List<DateTime>();


            foreach (
                DateTime fecha
                    in datos.Keys)
            {
                fechas.Add(
                    fecha);
            }


            fechas.Sort();


            return fechas;
        }


        // =====================================================
        // FORMATO
        // =====================================================

        private string FormatearFechaFlexible(
            DateTime fecha)
        {
            return fecha.ToString(
                "dddd d 'de' MMMM",
                new CultureInfo("es-AR"));
        }


        private string UnirListaFlexible(
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
