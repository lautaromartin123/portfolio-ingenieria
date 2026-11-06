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
        // ENTRADA A AGENDA INMEDIATA
        //
        // Orden:
        // 1. Agenda inmediata
        // 2. Histórico
        // 3. Chat general
        // =====================================================

        protected void BtnPreguntarIACompleto_Click(
            object sender,
            EventArgs e)
        {
            string pregunta =
                txtPreguntaIA.Text.Trim();


            if (!EsConsultaAgendaInmediata(
                pregunta))
            {
                BtnPreguntarIAHistorico_Click(
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
                    EjecutarConsultaAgendaInmediata(
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
                    "No se pudo consultar la agenda. "
                    +
                    Server.HtmlEncode(
                        ex.Message);
            }
        }


        // =====================================================
        // DETECTAR AGENDA INMEDIATA
        // =====================================================

        private bool EsConsultaAgendaInmediata(
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
                    "proximo turno")
                ||
                texto.Contains(
                    "siguiente turno"))
            {
                return true;
            }


            if (texto.Contains(
                    "manana")
                &&
                (
                    texto.Contains(
                        "que tengo")
                    ||
                    texto.Contains(
                        "turnos tengo")
                    ||
                    texto.Contains(
                        "turnos hay")
                    ||
                    texto.Contains(
                        "agenda")
                    ||
                    texto.Contains(
                        "primer turno")
                    ||
                    texto.Contains(
                        "ultimo turno")
                    ||
                    texto.Contains(
                        "a que hora termino")
                    ||
                    texto.Contains(
                        "hasta que hora")
                ))
            {
                return true;
            }


            if (texto.Contains(
                    "quedan")
                &&
                texto.Contains(
                    "turnos"))
            {
                return true;
            }


            if (texto.Contains(
                    "proximo dia")
                &&
                (
                    texto.Contains(
                        "sin turnos")
                    ||
                    texto.Contains(
                        "libre")
                    ||
                    texto.Contains(
                        "sin pacientes")
                ))
            {
                return true;
            }


            if (
                (
                    texto.Contains(
                        "mas carga")
                    ||
                    texto.Contains(
                        "mas turnos")
                    ||
                    texto.Contains(
                        "mas ocupado")
                )
                &&
                (
                    texto.Contains(
                        "semana que viene")
                    ||
                    texto.Contains(
                        "proxima semana")
                ))
            {
                return true;
            }


            if (texto.Contains(
                    "ultimo turno")
                &&
                texto.Contains(
                    "semana"))
            {
                return true;
            }


            return false;
        }


        // =====================================================
        // ROUTER
        // =====================================================

        private string EjecutarConsultaAgendaInmediata(
            string pregunta)
        {
            string texto =
                NormalizarTexto(
                    pregunta);


            if (texto.Contains(
                    "proximo turno")
                ||
                texto.Contains(
                    "siguiente turno"))
            {
                return ResponderProximoTurno();
            }


            if (texto.Contains(
                    "manana")
                &&
                texto.Contains(
                    "primer turno"))
            {
                return ResponderPrimerTurnoManana();
            }


            if (texto.Contains(
                    "manana")
                &&
                texto.Contains(
                    "ultimo turno"))
            {
                return ResponderUltimoTurnoManana();
            }


            if (texto.Contains(
                    "manana")
                &&
                (
                    texto.Contains(
                        "a que hora termino")
                    ||
                    texto.Contains(
                        "hasta que hora")
                ))
            {
                return ResponderHoraFinManana();
            }


            if (texto.Contains(
                "manana"))
            {
                return ResponderAgendaManana();
            }


            if (texto.Contains(
                    "quedan")
                &&
                texto.Contains(
                    "semana"))
            {
                return ResponderTurnosRestantesSemana();
            }


            if (texto.Contains(
                    "proximo dia")
                &&
                (
                    texto.Contains(
                        "sin turnos")
                    ||
                    texto.Contains(
                        "libre")
                    ||
                    texto.Contains(
                        "sin pacientes")
                ))
            {
                return ResponderProximoDiaLaboralLibre();
            }


            if (
                texto.Contains(
                    "semana que viene")
                ||
                texto.Contains(
                    "proxima semana"))
            {
                return ResponderDiaMasCargadoProximaSemana();
            }


            if (texto.Contains(
                    "ultimo turno")
                &&
                texto.Contains(
                    "semana"))
            {
                return ResponderUltimoTurnoSemana();
            }


            return
                "No pude identificar qué dato "
                +
                "de tu agenda querés consultar.";
        }


        // =====================================================
        // PROXIMO TURNO
        // =====================================================

        private string ResponderProximoTurno()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            TurnoAgendaIA turno =
                negocio.ObtenerProximoTurno(
                    IdProfesionalActual,
                    DateTime.Now);


            if (turno == null)
            {
                return
                    "No tenés turnos futuros agendados.";
            }


            return
                "Tu próximo turno es el "
                +
                FormatearFechaInmediata(
                    turno.FechaHora)
                +
                " a las "
                +
                turno.FechaHora.ToString(
                    "HH:mm")
                +
                ".";
        }


        // =====================================================
        // AGENDA MANANA
        // =====================================================

        private string ResponderAgendaManana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime manana =
                DateTime.Today.AddDays(1);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    manana,
                    manana.AddDays(1));


            if (turnos.Count == 0)
            {
                return
                    "Mañana no tenés turnos agendados.";
            }


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "Mañana tenés "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno."
                        : " turnos."
                ));


            texto.AppendLine();

            texto.AppendLine();


            texto.Append(
                "El primero es a las "
                +
                turnos[0]
                    .FechaHora
                    .ToString("HH:mm")
                +
                " y el último a las "
                +
                turnos[
                    turnos.Count - 1]
                    .FechaHora
                    .ToString("HH:mm")
                +
                ".");


            if (turnos.Count <= 8)
            {
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


                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "Horarios: "
                    +
                    UnirListaInmediata(
                        horarios)
                    +
                    ".");
            }


            return texto.ToString();
        }


        // =====================================================
        // PRIMER TURNO MANANA
        // =====================================================

        private string ResponderPrimerTurnoManana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime manana =
                DateTime.Today.AddDays(1);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    manana,
                    manana.AddDays(1));


            if (turnos.Count == 0)
            {
                return
                    "Mañana no tenés turnos agendados.";
            }


            return
                "Tu primer turno de mañana es a las "
                +
                turnos[0]
                    .FechaHora
                    .ToString("HH:mm")
                +
                ".";
        }


        // =====================================================
        // ULTIMO TURNO MANANA
        // =====================================================

        private string ResponderUltimoTurnoManana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime manana =
                DateTime.Today.AddDays(1);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    manana,
                    manana.AddDays(1));


            if (turnos.Count == 0)
            {
                return
                    "Mañana no tenés turnos agendados.";
            }


            TurnoAgendaIA ultimo =
                turnos[
                    turnos.Count - 1];


            return
                "Tu último turno de mañana empieza "
                +
                "a las "
                +
                ultimo.FechaHora
                    .ToString("HH:mm")
                +
                ".";
        }


        // =====================================================
        // HORA DE FIN MANANA
        // =====================================================

        private string ResponderHoraFinManana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime manana =
                DateTime.Today.AddDays(1);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    manana,
                    manana.AddDays(1));


            if (turnos.Count == 0)
            {
                return
                    "Mañana no tenés turnos agendados.";
            }


            TurnoAgendaIA ultimo =
                turnos[
                    turnos.Count - 1];


            return
                "Si no hay cambios en la agenda, "
                +
                "mañana terminás tu último turno "
                +
                "aproximadamente a las "
                +
                ultimo.FechaFin.ToString(
                    "HH:mm")
                +
                ".";
        }


        // =====================================================
        // RESTO SEMANA
        // =====================================================

        private string ResponderTurnosRestantesSemana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime ahora =
                DateTime.Now;


            DateTime lunes =
                ObtenerLunesSemana(
                    DateTime.Today);


            DateTime finSemana =
                lunes.AddDays(7);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    ahora,
                    finSemana);


            if (turnos.Count == 0)
            {
                return
                    "No te quedan turnos esta semana.";
            }


            TurnoAgendaIA proximo =
                turnos[0];


            return
                "Te quedan "
                +
                turnos.Count
                +
                (
                    turnos.Count == 1
                        ? " turno"
                        : " turnos"
                )
                +
                " esta semana. El próximo es el "
                +
                FormatearFechaInmediata(
                    proximo.FechaHora)
                +
                " a las "
                +
                proximo.FechaHora
                    .ToString("HH:mm")
                +
                ".";
        }


        // =====================================================
        // PROXIMO DIA LABORAL LIBRE
        // =====================================================

        private string ResponderProximoDiaLaboralLibre()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime? fecha =
                negocio
                    .ObtenerProximoDiaLaboralSinTurnos(
                        IdProfesionalActual,
                        DateTime.Today.AddDays(1),
                        60);


            if (!fecha.HasValue)
            {
                return
                    "No encontré un día laboral sin turnos "
                    +
                    "dentro de los próximos 60 días.";
            }


            return
                "Tu próximo día de trabajo sin turnos "
                +
                "es el "
                +
                FormatearFechaInmediata(
                    fecha.Value)
                +
                ".";
        }


        // =====================================================
        // MAS CARGADO PROXIMA SEMANA
        // =====================================================

        private string ResponderDiaMasCargadoProximaSemana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime desde =
                ObtenerLunesSemana(
                    DateTime.Today)
                .AddDays(7);


            DateTime hasta =
                desde.AddDays(7);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            if (turnos.Count == 0)
            {
                return
                    "La próxima semana no tenés turnos "
                    +
                    "agendados todavía.";
            }


            Dictionary<DateTime, int> cantidades =
                new Dictionary<DateTime, int>();


            foreach (
                TurnoAgendaIA turno
                    in turnos)
            {
                DateTime fecha =
                    turno.FechaHora.Date;


                if (!cantidades.ContainsKey(
                    fecha))
                {
                    cantidades.Add(
                        fecha,
                        0);
                }


                cantidades[fecha]++;
            }


            int maximo =
                0;


            foreach (
                KeyValuePair<DateTime, int> item
                    in cantidades)
            {
                if (item.Value >
                    maximo)
                {
                    maximo =
                        item.Value;
                }
            }


            List<string> dias =
                new List<string>();


            foreach (
                KeyValuePair<DateTime, int> item
                    in cantidades)
            {
                if (item.Value ==
                    maximo)
                {
                    dias.Add(
                        FormatearFechaInmediata(
                            item.Key));
                }
            }


            if (dias.Count == 1)
            {
                return
                    "El día con más carga la próxima semana "
                    +
                    "es el "
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
                "La mayor carga de la próxima semana "
                +
                "está empatada entre "
                +
                UnirListaInmediata(
                    dias)
                +
                ", con "
                +
                maximo
                +
                " turnos en cada día.";
        }


        // =====================================================
        // ULTIMO TURNO ESTA SEMANA
        // =====================================================

        private string ResponderUltimoTurnoSemana()
        {
            AgendaInmediataNegocio negocio =
                new AgendaInmediataNegocio();


            DateTime lunes =
                ObtenerLunesSemana(
                    DateTime.Today);


            DateTime finSemana =
                lunes.AddDays(7);


            List<TurnoAgendaIA> turnos =
                negocio.ObtenerTurnosPeriodo(
                    IdProfesionalActual,
                    DateTime.Now,
                    finSemana);


            if (turnos.Count == 0)
            {
                return
                    "No te quedan turnos esta semana.";
            }


            TurnoAgendaIA ultimo =
                turnos[
                    turnos.Count - 1];


            return
                "Tu último turno pendiente de esta semana "
                +
                "es el "
                +
                FormatearFechaInmediata(
                    ultimo.FechaHora)
                +
                " a las "
                +
                ultimo.FechaHora.ToString(
                    "HH:mm")
                +
                ".";
        }


        // =====================================================
        // FECHA NATURAL
        //
        // Se deja en minuscula para que quede:
        // "el lunes 26 de octubre"
        // =====================================================

        private string FormatearFechaInmediata(
            DateTime fecha)
        {
            return fecha.ToString(
                "dddd d 'de' MMMM",
                new CultureInfo("es-AR"));
        }


        // =====================================================
        // LISTA NATURAL
        // =====================================================

        private string UnirListaInmediata(
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
