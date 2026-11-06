using BIZ;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace Consultorios
{
    public partial class PanelIAMedico :
        System.Web.UI.Page
    {
        // =====================================================
        // PROFESIONAL ACTUAL
        // =====================================================

        private int IdProfesionalActual
        {
            get
            {
                if (ViewState["IdProfesionalActual"] == null)
                {
                    return 0;
                }

                return Convert.ToInt32(
                    ViewState["IdProfesionalActual"]);
            }

            set
            {
                ViewState["IdProfesionalActual"] =
                    value;
            }
        }


        // =====================================================
        // HISTORIAL
        // =====================================================

        private string HistorialChat
        {
            get
            {
                object historial =
                    Session[ObtenerClaveHistorial()];

                if (historial == null)
                {
                    return "";
                }

                return historial.ToString();
            }

            set
            {
                Session[ObtenerClaveHistorial()] =
                    value;
            }
        }


        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!Seguridad.RequerirRol(
                this,
                "MEDICO"))
            {
                return;
            }


            if (!Seguridad.RequerirAcceso(
                this))
            {
                return;
            }


            if (!IsPostBack)
            {
                CargarPanel();

                MostrarHistorialChat();
            }
        }


        // =====================================================
        // CARGAR PANEL
        // =====================================================

        private void CargarPanel()
        {
            lblMensaje.Text =
                "";

            pnlContenido.Visible =
                false;


            if (Session["IdUsuario"] == null)
            {
                lblMensaje.Text =
                    "No se pudo identificar al usuario.";

                return;
            }


            int idUsuario =
                Convert.ToInt32(
                    Session["IdUsuario"]);


            ProfesionalNegocio profesionalNegocio =
                new ProfesionalNegocio();


            Profesional profesional =
                profesionalNegocio
                    .BuscarPorUsuario(
                        idUsuario);


            if (profesional == null)
            {
                lblMensaje.Text =
                    "La cuenta médica no tiene "
                    +
                    "un profesional vinculado.";

                return;
            }


            IdProfesionalActual =
                profesional.IdProfesional;


            lblUsuario.Text =
                Server.HtmlEncode(
                    "Médico: "
                    +
                    profesional.Nombre
                    +
                    " "
                    +
                    profesional.Apellido);


            CargarEstadisticas();


            pnlContenido.Visible =
                true;
        }


        // =====================================================
        // ESTADÍSTICAS DEL PANEL
        // =====================================================

        private void CargarEstadisticas()
        {
            ReporteMedicoNegocio negocio =
                new ReporteMedicoNegocio();


            DiaMasAtendidoReporte dia =
                negocio.ObtenerDiaConMasAtendidos(
                    IdProfesionalActual);


            if (dia == null)
            {
                lblDiaMasAtendido.Text =
                    "Todavía no existen turnos "
                    +
                    "marcados como Atendido.";
            }
            else
            {
                lblDiaMasAtendido.Text =
                    Server.HtmlEncode(
                        dia.Fecha.ToString(
                            "dddd dd/MM/yyyy",
                            new CultureInfo("es-AR"))
                        +
                        " — "
                        +
                        dia.CantidadAtendidos
                        +
                        " pacientes atendidos.");
            }


            List<HorarioTrabajoReporte> horarios =
                negocio.ListarHorariosTrabajo(
                    IdProfesionalActual);


            litHorarios.Text =
                ConvertirAHtml(
                    ConstruirHorarios(
                        horarios));


            DateTime desde =
                ObtenerProximoLunes(
                    DateTime.Today);


            DateTime hasta =
                desde.AddDays(6);


            List<AgendaDiaReporte> agenda =
                negocio.ListarAgendaSemana(
                    IdProfesionalActual,
                    desde,
                    hasta.AddDays(1));


            lblPeriodo.Text =
                Server.HtmlEncode(
                    "Del "
                    +
                    desde.ToString("dd/MM/yyyy")
                    +
                    " al "
                    +
                    hasta.ToString("dd/MM/yyyy"));


            litAgenda.Text =
                ConvertirAHtml(
                    ConstruirAgenda(
                        agenda,
                        desde));
        }


        // =====================================================
        // REPORTE CON IA
        // =====================================================

        protected async void BtnGenerarIA_Click(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text =
                "";


            try
            {
                AsegurarProfesional();


                if (IdProfesionalActual <= 0)
                {
                    return;
                }


                btnGenerarIA.Enabled =
                    false;


                btnGenerarIA.Text =
                    "Generando reporte...";


                txtReporteIA.Text =
                    "Analizando datos...";


                string prompt =
                    ConstruirPromptIA();


                OllamaService ollama =
                    new OllamaService();


                string respuesta =
                    await ollama
                        .GenerarReporteAsync(
                            prompt);


                txtReporteIA.Text =
                    respuesta;
            }
            catch (Exception ex)
            {
                txtReporteIA.Text =
                    "";


                lblMensaje.Text =
                    "No se pudo generar el reporte. "
                    +
                    Server.HtmlEncode(
                        ex.Message);
            }
            finally
            {
                btnGenerarIA.Enabled =
                    true;


                btnGenerarIA.Text =
                    "Generar reporte con IA";
            }
        }


        // =====================================================
        // CHAT
        // =====================================================

        protected async void BtnPreguntarIA_Click(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text =
                "";


            string pregunta =
                txtPreguntaIA.Text.Trim();


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


            if (EsPreguntaClinica(
                pregunta))
            {
                AgregarAlHistorial(
                    "Médico",
                    pregunta);


                AgregarAlHistorial(
                    "IA",
                    "Puedo ayudarte con tu agenda, "
                    +
                    "horarios y estadísticas, pero no "
                    +
                    "con diagnósticos, medicamentos "
                    +
                    "o tratamientos.");


                txtPreguntaIA.Text =
                    "";


                MostrarHistorialChat();

                return;
            }


            try
            {
                AsegurarProfesional();


                if (IdProfesionalActual <= 0)
                {
                    return;
                }


                btnPreguntarIA.Enabled =
                    false;


                OllamaService ollama =
                    new OllamaService();


                string historialAnterior =
                    ObtenerHistorialParaIA();


                // =============================================
                // OLLAMA INTERPRETA LA INTENCIÓN
                // =============================================

                btnPreguntarIA.Text =
                    "Interpretando consulta...";


                ConsultaIA consulta =
                    await ollama
                        .InterpretarPreguntaAsync(
                            pregunta,
                            historialAnterior);


                // =============================================
                // C# VERIFICA INTENCIÓN Y PERÍODO
                // =============================================

                NormalizarConsultaPorTexto(
                    consulta,
                    pregunta);


                // =============================================
                // CONSULTAR DATOS
                // =============================================

                btnPreguntarIA.Text =
                    "Consultando datos...";


                string contexto =
                    EjecutarConsultaIA(
                        consulta,
                        pregunta);


                // =============================================
                // RESPUESTA
                // =============================================

                btnPreguntarIA.Text =
                    "Generando respuesta...";


                string respuesta;


                // Estas respuestas ya están redactadas
                // naturalmente por C# y contienen
                // datos determinísticos.

                if (consulta.Accion ==
                        "COMPARAR_PERIODOS"
                    ||
                    consulta.Accion ==
                        "DETECTAR_ALERTAS")
                {
                    respuesta =
                        contexto;
                }
                else
                {
                    respuesta =
                        await ollama
                            .ResponderPreguntaAsync(
                                contexto,
                                historialAnterior,
                                pregunta);
                }


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
                    "No se pudo procesar la consulta. "
                    +
                    Server.HtmlEncode(
                        ex.Message);
            }
            finally
            {
                btnPreguntarIA.Enabled =
                    true;


                btnPreguntarIA.Text =
                    "Preguntar a la IA";
            }
        }


        // =====================================================
        // NORMALIZAR CONSULTA
        // =====================================================

        private void NormalizarConsultaPorTexto(
            ConsultaIA consulta,
            string pregunta)
        {
            if (consulta == null)
            {
                return;
            }


            string texto =
                NormalizarTexto(
                    pregunta);


            // =================================================
            // ANÁLISIS DE AGENDA
            // =================================================

            if (texto.Contains(
                    "fuera de horario")
                ||
                texto.Contains(
                    "superpuesto")
                ||
                texto.Contains(
                    "superpuestos")
                ||
                texto.Contains(
                    "superposicion")
                ||
                texto.Contains(
                    "conflicto")
                ||
                texto.Contains(
                    "problema")
                ||
                texto.Contains(
                    "anomalia")
                ||
                texto.Contains(
                    "algo raro")
                ||
                texto.Contains(
                    "algo llamativo")
                ||
                texto.Contains(
                    "ocupacion")
                ||
                texto.Contains(
                    "capacidad")
                ||
                texto.Contains(
                    "hueco")
                ||
                texto.Contains(
                    "huecos")
                ||
                texto.Contains(
                    "tiempo libre")
                ||
                texto.Contains(
                    "dias sin pacientes")
                ||
                texto.Contains(
                    "dias sin turnos")
                ||
                texto.Contains(
                    "trabajo pero no tengo")
                ||
                texto.Contains(
                    "aprovechando bien")
                ||
                texto.Contains(
                    "aprovecho bien")
                ||
                texto.Contains(
                    "que tan ocupada")
                ||
                texto.Contains(
                    "que tan ocupado")
                ||
                texto.Contains(
                    "analiza mi agenda")
                ||
                texto.Contains(
                    "analizame la agenda")
                ||
                texto.Contains(
                    "revisa mi agenda")
                ||
                texto.Contains(
                    "como esta mi agenda"))
            {
                consulta.Accion =
                    "DETECTAR_ALERTAS";
            }


            // =================================================
            // COMPARACIONES
            // =================================================

            if (consulta.Accion ==
                "COMPARAR_PERIODOS")
            {
                if (texto.Contains(
                        "esta semana")
                    &&
                    (
                        texto.Contains(
                            "proxima")
                        ||
                        texto.Contains(
                            "semana que viene")
                    ))
                {
                    consulta.Periodo =
                        "ESTA_SEMANA";


                    consulta.Periodo2 =
                        "PROXIMA_SEMANA";


                    LimpiarFechasConsulta(
                        consulta);


                    return;
                }


                if (texto.Contains(
                        "semana pasada")
                    &&
                    texto.Contains(
                        "esta semana"))
                {
                    consulta.Periodo =
                        "SEMANA_PASADA";


                    consulta.Periodo2 =
                        "ESTA_SEMANA";


                    LimpiarFechasConsulta(
                        consulta);


                    return;
                }


                if (texto.Contains(
                        "este mes")
                    &&
                    (
                        texto.Contains(
                            "proximo")
                        ||
                        texto.Contains(
                            "mes que viene")
                    ))
                {
                    consulta.Periodo =
                        "ESTE_MES";


                    consulta.Periodo2 =
                        "PROXIMO_MES";


                    LimpiarFechasConsulta(
                        consulta);


                    return;
                }


                if (texto.Contains(
                        "mes pasado")
                    &&
                    texto.Contains(
                        "este mes"))
                {
                    consulta.Periodo =
                        "MES_PASADO";


                    consulta.Periodo2 =
                        "ESTE_MES";


                    LimpiarFechasConsulta(
                        consulta);


                    return;
                }
            }


            // =================================================
            // PERÍODOS INDIVIDUALES
            // =================================================

            if (texto.Contains(
                    "semana que viene")
                ||
                texto.Contains(
                    "proxima semana"))
            {
                consulta.Periodo =
                    "PROXIMA_SEMANA";

                return;
            }


            if (texto.Contains(
                "semana pasada"))
            {
                consulta.Periodo =
                    "SEMANA_PASADA";

                return;
            }


            if (texto.Contains(
                "esta semana"))
            {
                consulta.Periodo =
                    "ESTA_SEMANA";

                return;
            }


            if (texto.Contains(
                    "mes que viene")
                ||
                texto.Contains(
                    "proximo mes"))
            {
                consulta.Periodo =
                    "PROXIMO_MES";

                return;
            }


            if (texto.Contains(
                "mes pasado"))
            {
                consulta.Periodo =
                    "MES_PASADO";

                return;
            }


            if (texto.Contains(
                "este mes"))
            {
                consulta.Periodo =
                    "ESTE_MES";

                return;
            }


            if (texto.Contains(
                "manana"))
            {
                consulta.Periodo =
                    "MANANA";

                return;
            }


            if (texto.Contains(
                "hoy"))
            {
                consulta.Periodo =
                    "HOY";

                return;
            }


            if (texto.Contains(
                "ayer"))
            {
                consulta.Periodo =
                    "AYER";
            }
        }


        private void LimpiarFechasConsulta(
            ConsultaIA consulta)
        {
            consulta.Desde =
                "";

            consulta.Hasta =
                "";

            consulta.Desde2 =
                "";

            consulta.Hasta2 =
                "";
        }


        // =====================================================
        // NORMALIZAR TEXTO
        // =====================================================

        private string NormalizarTexto(
            string texto)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return "";
            }


            string normalizado =
                texto
                    .ToLowerInvariant()
                    .Normalize(
                        NormalizationForm.FormD);


            StringBuilder resultado =
                new StringBuilder();


            foreach (char caracter
                in normalizado)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            caracter);


                if (categoria !=
                    UnicodeCategory.NonSpacingMark)
                {
                    resultado.Append(
                        caracter);
                }
            }


            return resultado
                .ToString()
                .Normalize(
                    NormalizationForm.FormC);
        }


        // =====================================================
        // EJECUTAR CONSULTA
        // =====================================================

        private string EjecutarConsultaIA(
            ConsultaIA consulta,
            string pregunta)
        {
            if (consulta == null)
            {
                return
                    "No pude interpretar la consulta.";
            }


            switch (consulta.Accion)
            {
                case "AGENDA_PERIODO":

                    return EjecutarResumenPeriodo(
                        consulta,
                        "AGENDA DEL PERÍODO");


                case "RESUMEN_PERIODO":

                    return EjecutarResumenPeriodo(
                        consulta,
                        "RESUMEN DEL PERÍODO");


                case "DIA_MAS_CARGADO":

                    return EjecutarAnalisisCarga(
                        consulta,
                        true);


                case "DIA_MAS_LIBRE":

                    return EjecutarAnalisisCarga(
                        consulta,
                        false);


                case "COMPARAR_PERIODOS":

                    return EjecutarComparacion(
                        consulta);


                case "HORARIOS":

                    return EjecutarHorarios();


                case "ATENDIDOS":

                    return EjecutarAtendidos(
                        consulta);


                case "ESTADOS_TURNOS":

                    return EjecutarEstados(
                        consulta);


                case "DETECTAR_ALERTAS":

                    return EjecutarAlertas(
                        consulta,
                        pregunta);


                case "RESUMEN_GENERAL":

                    return EjecutarResumenPeriodo(
                        consulta,
                        "RESUMEN GENERAL");


                case "NO_SOPORTADO":

                    return
                        "Esa consulta no corresponde "
                        +
                        "a información operativa de agenda.";


                default:

                    return
                        "No pude identificar qué información "
                        +
                        "de la agenda querés consultar.";
            }
        }


        // =====================================================
        // RESUMEN
        // =====================================================

        private string EjecutarResumenPeriodo(
            ConsultaIA consulta,
            string titulo)
        {
            DateTime desde;
            DateTime hasta;


            ObtenerPeriodo(
                consulta,
                out desde,
                out hasta);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA resumen =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            return ConstruirResumenPeriodoTexto(
                resumen,
                titulo);
        }


        // =====================================================
        // CARGA
        // =====================================================

        private string EjecutarAnalisisCarga(
            ConsultaIA consulta,
            bool buscarMayor)
        {
            DateTime desde;
            DateTime hasta;


            ObtenerPeriodo(
                consulta,
                out desde,
                out hasta);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA resumen =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            int mejorCantidad =
                buscarMayor
                    ? -1
                    : int.MaxValue;


            List<DateTime> mejoresDias =
                new List<DateTime>();


            DateTime fecha =
                desde.Date;


            while (fecha <=
                   hasta.Date)
            {
                int cantidad =
                    ObtenerCantidadDia(
                        resumen,
                        fecha);


                bool reemplazar;


                if (buscarMayor)
                {
                    reemplazar =
                        cantidad >
                        mejorCantidad;
                }
                else
                {
                    reemplazar =
                        cantidad <
                        mejorCantidad;
                }


                if (reemplazar)
                {
                    mejorCantidad =
                        cantidad;


                    mejoresDias.Clear();


                    mejoresDias.Add(
                        fecha);
                }
                else if (cantidad ==
                         mejorCantidad)
                {
                    mejoresDias.Add(
                        fecha);
                }


                fecha =
                    fecha.AddDays(1);
            }


            StringBuilder texto =
                new StringBuilder();


            texto.AppendLine(
                "Período: "
                +
                desde.ToString("dd/MM/yyyy")
                +
                " al "
                +
                hasta.ToString("dd/MM/yyyy"));


            if (buscarMayor)
            {
                texto.AppendLine(
                    "Mayor cantidad de turnos: "
                    +
                    mejorCantidad);


                foreach (
                    DateTime dia
                        in mejoresDias)
                {
                    texto.AppendLine(
                        FormatearFechaNatural(
                            dia)
                        +
                        ": "
                        +
                        mejorCantidad
                        +
                        " turnos.");
                }
            }
            else
            {
                texto.AppendLine(
                    "Menor cantidad de turnos: "
                    +
                    mejorCantidad);


                foreach (
                    DateTime dia
                        in mejoresDias)
                {
                    texto.AppendLine(
                        FormatearFechaNatural(
                            dia)
                        +
                        ": "
                        +
                        mejorCantidad
                        +
                        " turnos.");
                }
            }


            return texto.ToString();
        }


        // =====================================================
        // COMPARACIÓN NATURAL
        // =====================================================

        private string EjecutarComparacion(
            ConsultaIA consulta)
        {
            DateTime desde1;
            DateTime hasta1;
            DateTime desde2;
            DateTime hasta2;


            ResolverPeriodo(
                consulta.Periodo,
                consulta.Desde,
                consulta.Hasta,
                out desde1,
                out hasta1);


            ResolverPeriodo(
                consulta.Periodo2,
                consulta.Desde2,
                consulta.Hasta2,
                out desde2,
                out hasta2);


            ValidarRango(
                desde1,
                hasta1);


            ValidarRango(
                desde2,
                hasta2);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA primero =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde1,
                    hasta1);


            ResumenPeriodoIA segundo =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde2,
                    hasta2);


            int diferencia =
                segundo.TurnosNoCancelados
                -
                primero.TurnosNoCancelados;


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "Del "
                +
                desde1.ToString("dd/MM/yyyy")
                +
                " al "
                +
                hasta1.ToString("dd/MM/yyyy")
                +
                " tenés "
                +
                primero.TurnosNoCancelados
                +
                " turnos activos. ");


            texto.Append(
                "Del "
                +
                desde2.ToString("dd/MM/yyyy")
                +
                " al "
                +
                hasta2.ToString("dd/MM/yyyy")
                +
                " tenés "
                +
                segundo.TurnosNoCancelados
                +
                ".");


            texto.AppendLine();

            texto.AppendLine();


            if (diferencia > 0)
            {
                texto.Append(
                    "El segundo período tiene "
                    +
                    diferencia
                    +
                    " turnos más.");
            }
            else if (diferencia < 0)
            {
                texto.Append(
                    "El segundo período tiene "
                    +
                    Math.Abs(diferencia)
                    +
                    " turnos menos.");
            }
            else
            {
                texto.Append(
                    "La cantidad de turnos es la misma "
                    +
                    "en ambos períodos.");
            }


            if (primero.Atendidos > 0 ||
                segundo.Atendidos > 0)
            {
                texto.Append(
                    " En cuanto a pacientes atendidos, "
                    +
                    "el primer período tiene "
                    +
                    primero.Atendidos
                    +
                    " y el segundo "
                    +
                    segundo.Atendidos
                    +
                    ".");
            }


            if (primero.Cancelados > 0 ||
                segundo.Cancelados > 0)
            {
                texto.Append(
                    " Hubo "
                    +
                    primero.Cancelados
                    +
                    " cancelaciones en el primero "
                    +
                    "y "
                    +
                    segundo.Cancelados
                    +
                    " en el segundo.");
            }


            return texto.ToString();
        }


        // =====================================================
        // HORARIOS
        // =====================================================

        private string EjecutarHorarios()
        {
            ReporteMedicoNegocio negocio =
                new ReporteMedicoNegocio();


            List<HorarioTrabajoReporte> horarios =
                negocio.ListarHorariosTrabajo(
                    IdProfesionalActual);


            return ConstruirHorarios(
                horarios);
        }


        // =====================================================
        // ATENDIDOS
        // =====================================================

        private string EjecutarAtendidos(
            ConsultaIA consulta)
        {
            DateTime desde;
            DateTime hasta;


            ObtenerPeriodo(
                consulta,
                out desde,
                out hasta);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA resumen =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            return
                "Período: "
                +
                desde.ToString("dd/MM/yyyy")
                +
                " al "
                +
                hasta.ToString("dd/MM/yyyy")
                +
                Environment.NewLine
                +
                "Pacientes atendidos: "
                +
                resumen.Atendidos
                +
                ".";
        }


        // =====================================================
        // ESTADOS
        // =====================================================

        private string EjecutarEstados(
            ConsultaIA consulta)
        {
            DateTime desde;
            DateTime hasta;


            ObtenerPeriodo(
                consulta,
                out desde,
                out hasta);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA resumen =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            if (resumen.Estados.Count == 0)
            {
                return
                    "No tenés turnos registrados "
                    +
                    "en ese período.";
            }


            StringBuilder texto =
                new StringBuilder();


            foreach (
                EstadoTurnoCantidadIA estado
                    in resumen.Estados)
            {
                texto.AppendLine(
                    estado.Estado
                    +
                    ": "
                    +
                    estado.Cantidad);
            }


            return texto.ToString();
        }


        // =====================================================
        // RESPUESTAS HUMANAS DE ANÁLISIS DE AGENDA
        // =====================================================

        private string EjecutarAlertas(
            ConsultaIA consulta,
            string pregunta)
        {
            DateTime desde;
            DateTime hasta;


            ObtenerPeriodo(
                consulta,
                out desde,
                out hasta);


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


            string textoPregunta =
                NormalizarTexto(
                    pregunta);


            // =================================================
            // SUPERPOSICIONES
            // =================================================

            if (textoPregunta.Contains(
                    "superpuesto")
                ||
                textoPregunta.Contains(
                    "superpuestos")
                ||
                textoPregunta.Contains(
                    "superposicion"))
            {
                return ResponderSuperposiciones(
                    consulta,
                    desde,
                    hasta,
                    analisis);
            }


            // =================================================
            // FUERA DE HORARIO
            // =================================================

            if (textoPregunta.Contains(
                "fuera de horario"))
            {
                return ResponderFueraDeHorario(
                    consulta,
                    desde,
                    hasta,
                    analisis);
            }


            // =================================================
            // MAYOR HUECO
            // =================================================

            if (textoPregunta.Contains(
                "mayor hueco"))
            {
                return ResponderMayorHueco(
                    consulta,
                    desde,
                    hasta,
                    avanzado);
            }


            // =================================================
            // HUECOS
            // =================================================

            if (textoPregunta.Contains(
                    "hueco")
                ||
                textoPregunta.Contains(
                    "huecos"))
            {
                return ResponderHuecos(
                    consulta,
                    desde,
                    hasta,
                    avanzado);
            }


            // =================================================
            // DÍAS LABORALES SIN TURNOS
            // =================================================

            if (textoPregunta.Contains(
                    "dias sin pacientes")
                ||
                textoPregunta.Contains(
                    "dias sin turnos")
                ||
                textoPregunta.Contains(
                    "trabajo pero no tengo"))
            {
                return ResponderDiasSinTurnos(
                    consulta,
                    desde,
                    hasta,
                    avanzado);
            }


            // =================================================
            // OCUPACIÓN
            // =================================================

            if (textoPregunta.Contains(
                    "ocupacion")
                ||
                textoPregunta.Contains(
                    "capacidad")
                ||
                textoPregunta.Contains(
                    "aprovechando bien")
                ||
                textoPregunta.Contains(
                    "aprovecho bien")
                ||
                textoPregunta.Contains(
                    "que tan ocupada")
                ||
                textoPregunta.Contains(
                    "que tan ocupado"))
            {
                return ResponderOcupacion(
                    consulta,
                    desde,
                    hasta,
                    avanzado);
            }


            // =================================================
            // CONSULTA GENERAL
            // =================================================

            return ResponderAnalisisGeneral(
                consulta,
                desde,
                hasta,
                resumen,
                analisis,
                avanzado);
        }


        // =====================================================
        // RESPUESTA: OCUPACIÓN
        // =====================================================

        private string ResponderOcupacion(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            AnalisisAvanzadoAgendaIA avanzado)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            if (avanzado.CapacidadTeoricaTurnos <= 0)
            {
                return
                    "No puedo calcular la ocupación "
                    +
                    periodo
                    +
                    " porque no tenés capacidad de "
                    +
                    "turnos configurada en esos horarios.";
            }


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "Tu ocupación "
                +
                periodo
                +
                " es del "
                +
                FormatearPorcentaje(
                    avanzado.PorcentajeOcupacion)
                +
                ". ");


            texto.Append(
                "Tenés "
                +
                avanzado.TurnosDentroHorarioLaboral
                +
                " turnos dentro de horario y, "
                +
                "según la duración configurada "
                +
                "de cada turno, podrías cubrir hasta "
                +
                avanzado.CapacidadTeoricaTurnos
                +
                ".");


            if (avanzado.DiasLaborables > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                if (avanzado.DiasLaborablesSinTurnos == 0)
                {
                    texto.Append(
                        "Tus "
                        +
                        avanzado.DiasLaborables
                        +
                        " días laborales del período "
                        +
                        "tienen al menos un turno.");
                }
                else
                {
                    texto.Append(
                        "Trabajás "
                        +
                        avanzado.DiasLaborables
                        +
                        " días en ese período y "
                        +
                        avanzado.DiasLaborablesSinTurnos
                        +
                        " todavía "
                        +
                        (
                            avanzado.DiasLaborablesSinTurnos == 1
                                ? "no tiene"
                                : "no tienen"
                        )
                        +
                        " turnos.");
                }
            }


            if (avanzado.PorcentajeOcupacion < 25)
            {
                texto.AppendLine();

                texto.AppendLine();

                texto.Append(
                    "La agenda está bastante libre.");
            }
            else if (avanzado.PorcentajeOcupacion < 60)
            {
                texto.AppendLine();

                texto.AppendLine();

                texto.Append(
                    "La agenda tiene una ocupación moderada.");
            }
            else if (avanzado.PorcentajeOcupacion < 90)
            {
                texto.AppendLine();

                texto.AppendLine();

                texto.Append(
                    "La agenda está bastante ocupada.");
            }
            else
            {
                texto.AppendLine();

                texto.AppendLine();

                texto.Append(
                    "La agenda está casi al límite "
                    +
                    "de la capacidad configurada.");
            }


            return texto.ToString();
        }


        // =====================================================
        // RESPUESTA: DÍAS SIN TURNOS
        // =====================================================

        private string ResponderDiasSinTurnos(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            AnalisisAvanzadoAgendaIA avanzado)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            List<string> dias =
                new List<string>();


            foreach (
                AlertaAvanzadaAgendaIA alerta
                    in avanzado.Alertas)
            {
                if (alerta.Tipo ==
                        "DIA_LABORABLE_SIN_TURNOS"
                    &&
                    alerta.FechaHora.HasValue)
                {
                    dias.Add(
                        FormatearFechaNatural(
                            alerta.FechaHora.Value));
                }
            }


            if (dias.Count == 0)
            {
                return
                    "No. "
                    +
                    CapitalizarPrimera(
                        periodo)
                    +
                    " no tenés días laborales "
                    +
                    "completamente libres.";
            }


            if (dias.Count == 1)
            {
                return
                    CapitalizarPrimera(
                        periodo)
                    +
                    " tenés un día de trabajo "
                    +
                    "sin turnos: "
                    +
                    dias[0]
                    +
                    ".";
            }


            return
                CapitalizarPrimera(
                    periodo)
                +
                " tenés "
                +
                dias.Count
                +
                " días de trabajo sin turnos: "
                +
                UnirListaNatural(
                    dias)
                +
                ".";
        }


        // =====================================================
        // RESPUESTA: HUECOS
        // =====================================================

        private string ResponderHuecos(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            AnalisisAvanzadoAgendaIA avanzado)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            if (avanzado.HuecosLargos == 0)
            {
                return
                    "No detecté huecos de una hora "
                    +
                    "o más entre turnos "
                    +
                    periodo
                    +
                    ".";
            }


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "Sí. Detecté "
                +
                avanzado.HuecosLargos
                +
                (
                    avanzado.HuecosLargos == 1
                        ? " hueco de una hora o más "
                        : " huecos de una hora o más "
                )
                +
                periodo
                +
                ". ");


            texto.Append(
                "En total suman "
                +
                avanzado.MinutosHuecosTotales
                +
                " minutos.");


            if (avanzado.MayorHuecoMinutos > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "El mayor es de "
                    +
                    avanzado.MayorHuecoMinutos
                    +
                    " minutos");


                if (avanzado.FechaMayorHueco.HasValue)
                {
                    texto.Append(
                        " y aparece el "
                        +
                        FormatearFechaNatural(
                            avanzado
                                .FechaMayorHueco
                                .Value));
                }


                texto.Append(".");
            }


            return texto.ToString();
        }


        // =====================================================
        // RESPUESTA: MAYOR HUECO
        // =====================================================

        private string ResponderMayorHueco(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            AnalisisAvanzadoAgendaIA avanzado)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            if (avanzado.MayorHuecoMinutos <= 0)
            {
                return
                    "No detecté huecos de una hora "
                    +
                    "o más entre turnos "
                    +
                    periodo
                    +
                    ".";
            }


            string respuesta =
                "Tu mayor hueco "
                +
                periodo
                +
                " es de "
                +
                avanzado.MayorHuecoMinutos
                +
                " minutos";


            if (avanzado.FechaMayorHueco.HasValue)
            {
                respuesta +=
                    " y se da el "
                    +
                    FormatearFechaNatural(
                        avanzado
                            .FechaMayorHueco
                            .Value);
            }


            respuesta +=
                ".";


            if (avanzado.HuecosLargos > 1)
            {
                respuesta +=
                    " En total detecté "
                    +
                    avanzado.HuecosLargos
                    +
                    " huecos de una hora o más.";
            }


            return respuesta;
        }


        // =====================================================
        // RESPUESTA: SUPERPOSICIONES
        // =====================================================

        private string ResponderSuperposiciones(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            AnalisisAgendaIA analisis)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            if (analisis.Superposiciones == 0)
            {
                return
                    "No. No tenés turnos superpuestos "
                    +
                    periodo
                    +
                    ".";
            }


            StringBuilder texto =
                new StringBuilder();


            texto.Append(
                "Sí. Detecté "
                +
                analisis.Superposiciones
                +
                (
                    analisis.Superposiciones == 1
                        ? " superposición "
                        : " superposiciones "
                )
                +
                periodo
                +
                ".");


            List<string> momentos =
                new List<string>();


            foreach (
                AlertaAgendaIA alerta
                    in analisis.Alertas)
            {
                if (alerta.Tipo ==
                        "SUPERPOSICION"
                    &&
                    alerta.FechaHora.HasValue)
                {
                    momentos.Add(
                        FormatearFechaHoraNatural(
                            alerta.FechaHora.Value));
                }
            }


            if (momentos.Count > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "Las encontré alrededor de "
                    +
                    UnirListaNatural(
                        momentos)
                    +
                    ".");
            }


            return texto.ToString();
        }


        // =====================================================
        // RESPUESTA: FUERA DE HORARIO
        // =====================================================

        private string ResponderFueraDeHorario(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            AnalisisAgendaIA analisis)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            int problemas =
                analisis.TurnosFueraHorario
                +
                analisis.TurnosEnDiaNoLaborable;


            if (problemas == 0)
            {
                return
                    "No. Todos tus turnos "
                    +
                    periodo
                    +
                    " están dentro de los días "
                    +
                    "y horarios de trabajo configurados.";
            }


            StringBuilder texto =
                new StringBuilder();


            if (analisis.TurnosFueraHorario > 0)
            {
                texto.Append(
                    "Tenés "
                    +
                    analisis.TurnosFueraHorario
                    +
                    (
                        analisis.TurnosFueraHorario == 1
                            ? " turno fuera del horario laboral."
                            : " turnos fuera del horario laboral."
                    ));
            }


            if (analisis.TurnosEnDiaNoLaborable > 0)
            {
                if (texto.Length > 0)
                {
                    texto.Append(" ");
                }


                texto.Append(
                    "Además, hay "
                    +
                    analisis.TurnosEnDiaNoLaborable
                    +
                    (
                        analisis.TurnosEnDiaNoLaborable == 1
                            ? " turno en un día en el que no trabajás."
                            : " turnos en días en los que no trabajás."
                    ));
            }


            List<string> detalles =
                new List<string>();


            foreach (
                AlertaAgendaIA alerta
                    in analisis.Alertas)
            {
                if (alerta.Tipo ==
                        "FUERA_DE_HORARIO"
                    ||
                    alerta.Tipo ==
                        "DIA_NO_LABORABLE")
                {
                    detalles.Add(
                        alerta.Descripcion);
                }
            }


            if (detalles.Count > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                int limite =
                    Math.Min(
                        detalles.Count,
                        3);


                for (int i = 0;
                     i < limite;
                     i++)
                {
                    texto.AppendLine(
                        detalles[i]);
                }


                if (detalles.Count > limite)
                {
                    texto.Append(
                        "Hay "
                        +
                        (
                            detalles.Count
                            -
                            limite
                        )
                        +
                        " casos más.");
                }
            }


            return texto
                .ToString()
                .Trim();
        }


        // =====================================================
        // RESPUESTA: ANÁLISIS GENERAL
        // =====================================================

        private string ResponderAnalisisGeneral(
            ConsultaIA consulta,
            DateTime desde,
            DateTime hasta,
            ResumenPeriodoIA resumen,
            AnalisisAgendaIA analisis,
            AnalisisAvanzadoAgendaIA avanzado)
        {
            string periodo =
                DescribirPeriodo(
                    consulta.Periodo,
                    desde,
                    hasta);


            StringBuilder texto =
                new StringBuilder();


            // =================================================
            // CARGA GENERAL
            // =================================================

            if (resumen.TurnosNoCancelados == 0)
            {
                texto.Append(
                    "No tenés turnos activos "
                    +
                    periodo
                    +
                    ".");
            }
            else
            {
                texto.Append(
                    CapitalizarPrimera(
                        periodo)
                    +
                    " tenés "
                    +
                    resumen.TurnosNoCancelados
                    +
                    (
                        resumen.TurnosNoCancelados == 1
                            ? " turno activo."
                            : " turnos activos."
                    ));
            }


            // =================================================
            // OCUPACIÓN
            // =================================================

            if (avanzado.CapacidadTeoricaTurnos > 0)
            {
                texto.Append(
                    " La ocupación de tu agenda es del "
                    +
                    FormatearPorcentaje(
                        avanzado.PorcentajeOcupacion)
                    +
                    ".");


                if (avanzado.PorcentajeOcupacion < 25)
                {
                    texto.Append(
                        " Está bastante libre.");
                }
                else if (avanzado.PorcentajeOcupacion < 60)
                {
                    texto.Append(
                        " La carga es moderada.");
                }
                else if (avanzado.PorcentajeOcupacion < 90)
                {
                    texto.Append(
                        " Está bastante ocupada.");
                }
                else
                {
                    texto.Append(
                        " Está prácticamente al límite "
                        +
                        "de la capacidad configurada.");
                }
            }


            // =================================================
            // DÍAS SIN TURNOS
            // =================================================

            if (avanzado.DiasLaborablesSinTurnos > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "De tus "
                    +
                    avanzado.DiasLaborables
                    +
                    " días laborales, "
                    +
                    avanzado.DiasLaborablesSinTurnos
                    +
                    (
                        avanzado.DiasLaborablesSinTurnos == 1
                            ? " todavía no tiene turnos."
                            : " todavía no tienen turnos."
                    ));
            }


            // =================================================
            // PROBLEMAS DE HORARIO
            // =================================================

            texto.AppendLine();

            texto.AppendLine();


            if (analisis.TurnosFueraHorario == 0 &&
                analisis.TurnosEnDiaNoLaborable == 0 &&
                analisis.Superposiciones == 0)
            {
                texto.Append(
                    "No encontré turnos fuera de horario "
                    +
                    "ni superposiciones.");
            }
            else
            {
                List<string> problemas =
                    new List<string>();


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


                texto.Append(
                    "Sí encontré algo para revisar: "
                    +
                    UnirListaNatural(
                        problemas)
                    +
                    ".");
            }


            // =================================================
            // HUECOS
            // =================================================

            if (avanzado.HuecosLargos > 0)
            {
                texto.AppendLine();

                texto.AppendLine();


                texto.Append(
                    "También detecté "
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

            if (resumen.TurnosNoCancelados >= 4 &&
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
                        " turnos están el "
                        +
                        FormatearFechaNatural(
                            resumen
                                .DiaMasCargado
                                .Value)
                        +
                        ".");
                }
            }


            // =================================================
            // CANCELACIONES
            // =================================================

            if (resumen.TotalTurnos > 0)
            {
                decimal porcentajeCancelados =
                    (
                        (decimal)resumen.Cancelados
                        /
                        resumen.TotalTurnos
                    )
                    *
                    100;


                if (porcentajeCancelados >= 30)
                {
                    texto.AppendLine();

                    texto.AppendLine();


                    texto.Append(
                        "Además, las cancelaciones son "
                        +
                        "relativamente altas: "
                        +
                        FormatearPorcentaje(
                            porcentajeCancelados)
                        +
                        " de los turnos del período.");
                }
            }


            return texto.ToString();
        }


        // =====================================================
        // PERÍODOS
        // =====================================================

        private void ObtenerPeriodo(
            ConsultaIA consulta,
            out DateTime desde,
            out DateTime hasta)
        {
            string periodo =
                consulta.Periodo;


            if (string.IsNullOrWhiteSpace(
                    periodo)
                &&
                !string.IsNullOrWhiteSpace(
                    consulta.Desde))
            {
                periodo =
                    "RANGO_EXPLICITO";
            }


            ResolverPeriodo(
                periodo,
                consulta.Desde,
                consulta.Hasta,
                out desde,
                out hasta);


            ValidarRango(
                desde,
                hasta);
        }


        private void ResolverPeriodo(
            string periodo,
            string desdeTexto,
            string hastaTexto,
            out DateTime desde,
            out DateTime hasta)
        {
            DateTime hoy =
                DateTime.Today;


            string tipo =
                periodo == null
                    ? ""
                    : periodo
                        .Trim()
                        .ToUpperInvariant();


            switch (tipo)
            {
                case "HOY":

                    desde =
                        hoy;

                    hasta =
                        hoy;

                    return;


                case "MANANA":

                    desde =
                        hoy.AddDays(1);

                    hasta =
                        desde;

                    return;


                case "AYER":

                    desde =
                        hoy.AddDays(-1);

                    hasta =
                        desde;

                    return;


                case "ESTA_SEMANA":

                    desde =
                        ObtenerLunesSemana(
                            hoy);

                    hasta =
                        desde.AddDays(6);

                    return;


                case "PROXIMA_SEMANA":

                    desde =
                        ObtenerLunesSemana(
                            hoy)
                        .AddDays(7);

                    hasta =
                        desde.AddDays(6);

                    return;


                case "SEMANA_PASADA":

                    desde =
                        ObtenerLunesSemana(
                            hoy)
                        .AddDays(-7);

                    hasta =
                        desde.AddDays(6);

                    return;


                case "ESTE_MES":

                    desde =
                        new DateTime(
                            hoy.Year,
                            hoy.Month,
                            1);

                    hasta =
                        desde
                            .AddMonths(1)
                            .AddDays(-1);

                    return;


                case "PROXIMO_MES":

                    desde =
                        new DateTime(
                            hoy.Year,
                            hoy.Month,
                            1)
                        .AddMonths(1);

                    hasta =
                        desde
                            .AddMonths(1)
                            .AddDays(-1);

                    return;


                case "MES_PASADO":

                    desde =
                        new DateTime(
                            hoy.Year,
                            hoy.Month,
                            1)
                        .AddMonths(-1);

                    hasta =
                        desde
                            .AddMonths(1)
                            .AddDays(-1);

                    return;


                case "FECHA_EXPLICITA":
                case "RANGO_EXPLICITO":

                    if (!IntentarFecha(
                        desdeTexto,
                        out desde))
                    {
                        throw new Exception(
                            "No se pudo interpretar "
                            +
                            "la fecha inicial.");
                    }


                    if (!IntentarFecha(
                        hastaTexto,
                        out hasta))
                    {
                        hasta =
                            desde;
                    }


                    return;


                case "SIN_PERIODO":
                default:

                    desde =
                        ObtenerLunesSemana(
                            hoy);

                    hasta =
                        desde.AddDays(6);

                    return;
            }
        }


        private DateTime ObtenerLunesSemana(
            DateTime fecha)
        {
            int diferencia =
                (
                    7
                    +
                    (
                        (int)fecha.DayOfWeek
                        -
                        (int)DayOfWeek.Monday
                    )
                )
                %
                7;


            return fecha.Date
                .AddDays(
                    -diferencia);
        }


        private bool IntentarFecha(
            string valor,
            out DateTime fecha)
        {
            return DateTime.TryParseExact(
                valor,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha);
        }


        private void ValidarRango(
            DateTime desde,
            DateTime hasta)
        {
            if (hasta.Date <
                desde.Date)
            {
                throw new Exception(
                    "El período interpretado "
                    +
                    "no es válido.");
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
                throw new Exception(
                    "El período solicitado supera "
                    +
                    "los 366 días permitidos.");
            }
        }


        // =====================================================
        // RESUMEN PARA OLLAMA
        // =====================================================

        private string ConstruirResumenPeriodoTexto(
            ResumenPeriodoIA resumen,
            string titulo)
        {
            StringBuilder texto =
                new StringBuilder();


            texto.AppendLine(
                titulo);


            texto.AppendLine(
                "Período: "
                +
                resumen.Desde.ToString("dd/MM/yyyy")
                +
                " al "
                +
                resumen.Hasta.ToString("dd/MM/yyyy"));


            texto.AppendLine(
                "Turnos registrados: "
                +
                resumen.TotalTurnos);


            texto.AppendLine(
                "Turnos activos: "
                +
                resumen.TurnosNoCancelados);


            texto.AppendLine(
                "Atendidos: "
                +
                resumen.Atendidos);


            texto.AppendLine(
                "Cancelados: "
                +
                resumen.Cancelados);


            texto.AppendLine(
                "Días con turnos: "
                +
                resumen.DiasConTurnos);


            texto.AppendLine(
                "Promedio por día con actividad: "
                +
                resumen
                    .PromedioTurnosPorDiaConActividad
                    .ToString("0.00"));


            if (resumen.DiaMasCargado.HasValue)
            {
                texto.AppendLine(
                    "Día más cargado: "
                    +
                    resumen
                        .DiaMasCargado
                        .Value
                        .ToString("dd/MM/yyyy")
                    +
                    " con "
                    +
                    resumen.CantidadDiaMasCargado
                    +
                    " turnos.");
            }


            return texto.ToString();
        }


        // =====================================================
        // BUSCAR DÍA
        // =====================================================

        private DiaAgendaIA BuscarDia(
            ResumenPeriodoIA resumen,
            DateTime fecha)
        {
            foreach (
                DiaAgendaIA dia
                    in resumen.Dias)
            {
                if (dia.Fecha.Date ==
                    fecha.Date)
                {
                    return dia;
                }
            }


            return null;
        }


        private int ObtenerCantidadDia(
            ResumenPeriodoIA resumen,
            DateTime fecha)
        {
            DiaAgendaIA dia =
                BuscarDia(
                    resumen,
                    fecha);


            if (dia == null)
            {
                return 0;
            }


            return dia.CantidadTurnos;
        }


        // =====================================================
        // REPORTE AUTOMÁTICO
        // =====================================================

        private string ConstruirPromptIA()
        {
            DateTime desde =
                ObtenerLunesSemana(
                    DateTime.Today)
                .AddDays(7);


            DateTime hasta =
                desde.AddDays(6);


            AsistenteMedicoNegocio negocio =
                new AsistenteMedicoNegocio();


            ResumenPeriodoIA resumen =
                negocio.ObtenerResumenPeriodo(
                    IdProfesionalActual,
                    desde,
                    hasta);


            StringBuilder texto =
                new StringBuilder();


            texto.AppendLine(
                ConstruirResumenPeriodoTexto(
                    resumen,
                    "PRÓXIMA SEMANA"));


            texto.AppendLine();


            texto.AppendLine(
                "Explicá esta información de forma "
                +
                "natural, breve y útil para el médico.");


            texto.AppendLine(
                "No muestres nombres internos del sistema.");


            texto.AppendLine(
                "No uses frases como 'calculado por C#'.");


            texto.AppendLine(
                "El valor 0 es información válida.");


            texto.AppendLine(
                "No inventes fechas ni cantidades.");


            return texto.ToString();
        }


        // =====================================================
        // BLOQUEO CLÍNICO
        // =====================================================

        private bool EsPreguntaClinica(
            string pregunta)
        {
            string texto =
                pregunta
                    .ToLowerInvariant();


            string[] palabras =
            {
                "diagnóstico",
                "diagnostico",
                "medicamento",
                "medicación",
                "medicacion",
                "dosis",
                "tratamiento",
                "recetar",
                "receta médica",
                "receta medica",
                "qué enfermedad",
                "que enfermedad",
                "qué tiene el paciente",
                "que tiene el paciente",
                "síntoma",
                "sintoma"
            };


            foreach (
                string palabra
                    in palabras)
            {
                if (texto.Contains(
                    palabra))
                {
                    return true;
                }
            }


            return false;
        }


        // =====================================================
        // HISTORIAL
        // =====================================================

        private void AgregarAlHistorial(
            string autor,
            string mensaje)
        {
            StringBuilder historial =
                new StringBuilder(
                    HistorialChat);


            if (historial.Length > 0)
            {
                historial.AppendLine();

                historial.AppendLine();
            }


            historial.AppendLine(
                autor + ":");


            historial.AppendLine(
                mensaje);


            HistorialChat =
                historial.ToString();
        }


        private void MostrarHistorialChat()
        {
            txtHistorialChat.Text =
                HistorialChat;
        }


        private string ObtenerHistorialParaIA()
        {
            string historial =
                HistorialChat;


            const int maximoCaracteres =
                6000;


            if (historial.Length <=
                maximoCaracteres)
            {
                return historial;
            }


            return historial.Substring(
                historial.Length
                -
                maximoCaracteres);
        }


        private string ObtenerClaveHistorial()
        {
            return
                "PanelIAMedico_Historial_"
                +
                Convert.ToString(
                    Session["IdUsuario"]);
        }


        // =====================================================
        // LIMPIAR CHAT
        // =====================================================

        protected void BtnLimpiarChat_Click(
            object sender,
            EventArgs e)
        {
            HistorialChat =
                "";


            txtHistorialChat.Text =
                "";


            txtPreguntaIA.Text =
                "";


            lblMensaje.Text =
                "Conversación eliminada.";
        }


        // =====================================================
        // PROFESIONAL
        // =====================================================

        private void AsegurarProfesional()
        {
            if (IdProfesionalActual > 0)
            {
                return;
            }


            CargarPanel();


            if (IdProfesionalActual <= 0)
            {
                lblMensaje.Text =
                    "No se pudo identificar al profesional.";
            }
        }


        // =====================================================
        // HORARIOS
        // =====================================================

        private string ConstruirHorarios(
            List<HorarioTrabajoReporte> horarios)
        {
            if (horarios == null ||
                horarios.Count == 0)
            {
                return
                    "No hay horarios de trabajo configurados.";
            }


            StringBuilder texto =
                new StringBuilder();


            foreach (
                HorarioTrabajoReporte horario
                    in horarios)
            {
                texto.AppendLine(
                    horario.NombreDia
                    +
                    ": "
                    +
                    horario.HoraInicio
                        .ToString(@"hh\:mm")
                    +
                    " a "
                    +
                    horario.HoraFin
                        .ToString(@"hh\:mm"));
            }


            return texto.ToString();
        }


        // =====================================================
        // AGENDA
        // =====================================================

        private string ConstruirAgenda(
            List<AgendaDiaReporte> agenda,
            DateTime desde)
        {
            StringBuilder texto =
                new StringBuilder();


            CultureInfo cultura =
                new CultureInfo("es-AR");


            for (int i = 0;
                 i < 7;
                 i++)
            {
                DateTime fecha =
                    desde.AddDays(i);


                AgendaDiaReporte encontrado =
                    null;


                if (agenda != null)
                {
                    foreach (
                        AgendaDiaReporte item
                            in agenda)
                    {
                        if (item.Fecha.Date ==
                            fecha.Date)
                        {
                            encontrado =
                                item;

                            break;
                        }
                    }
                }


                if (encontrado == null)
                {
                    texto.AppendLine(
                        fecha.ToString(
                            "dddd dd/MM",
                            cultura)
                        +
                        ": sin turnos.");
                }
                else
                {
                    texto.AppendLine(
                        fecha.ToString(
                            "dddd dd/MM",
                            cultura)
                        +
                        ": "
                        +
                        encontrado.CantidadTurnos
                        +
                        " turnos. "
                        +
                        "Primer turno: "
                        +
                        encontrado.PrimerTurno
                            .ToString("HH:mm")
                        +
                        ". Último turno: "
                        +
                        encontrado.UltimoTurno
                            .ToString("HH:mm")
                        +
                        ".");
                }
            }


            return texto.ToString();
        }


        // =====================================================
        // TEXTO NATURAL
        // =====================================================

        private string DescribirPeriodo(
            string periodo,
            DateTime desde,
            DateTime hasta)
        {
            string tipo =
                periodo == null
                    ? ""
                    : periodo.ToUpperInvariant();


            switch (tipo)
            {
                case "HOY":
                    return "hoy";

                case "MANANA":
                    return "mañana";

                case "AYER":
                    return "ayer";

                case "ESTA_SEMANA":
                    return "esta semana";

                case "PROXIMA_SEMANA":
                    return "la próxima semana";

                case "SEMANA_PASADA":
                    return "la semana pasada";

                case "ESTE_MES":
                    return "este mes";

                case "PROXIMO_MES":
                    return "el próximo mes";

                case "MES_PASADO":
                    return "el mes pasado";

                default:

                    if (desde.Date ==
                        hasta.Date)
                    {
                        return
                            "el "
                            +
                            desde.ToString(
                                "dd/MM/yyyy");
                    }


                    return
                        "entre el "
                        +
                        desde.ToString("dd/MM/yyyy")
                        +
                        " y el "
                        +
                        hasta.ToString("dd/MM/yyyy");
            }
        }


        private string FormatearFechaNatural(
            DateTime fecha)
        {
            string resultado =
                fecha.ToString(
                    "dddd d 'de' MMMM",
                    new CultureInfo("es-AR"));


            return CapitalizarPrimera(
                resultado);
        }


        private string FormatearFechaHoraNatural(
            DateTime fecha)
        {
            return
                FormatearFechaNatural(
                    fecha)
                +
                " a las "
                +
                fecha.ToString("HH:mm");
        }


        private string FormatearPorcentaje(
            decimal valor)
        {
            return valor.ToString(
                "0.0",
                new CultureInfo("es-AR"))
                +
                "%";
        }


        private string CapitalizarPrimera(
            string texto)
        {
            if (string.IsNullOrEmpty(
                texto))
            {
                return texto;
            }


            if (texto.Length == 1)
            {
                return texto.ToUpper();
            }


            return
                char.ToUpper(
                    texto[0])
                +
                texto.Substring(1);
        }


        private string UnirListaNatural(
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


        private DateTime ObtenerProximoLunes(
            DateTime fecha)
        {
            return ObtenerLunesSemana(
                fecha)
                .AddDays(7);
        }


        private string ConvertirAHtml(
            string texto)
        {
            return HttpUtility
                .HtmlEncode(texto)
                .Replace(
                    Environment.NewLine,
                    "<br />");
        }
    }
}
