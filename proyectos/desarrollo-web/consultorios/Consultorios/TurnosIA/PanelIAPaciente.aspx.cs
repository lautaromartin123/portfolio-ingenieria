using BIZ;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Consultorios
{
    public partial class PanelIAPaciente : System.Web.UI.Page
    {
        private const string SESION_ESTADO = "IA_PACIENTE_ESTADO";
        private const string SESION_MENSAJES = "IA_PACIENTE_MENSAJES";
        private const string SESION_OPCIONES = "IA_PACIENTE_OPCIONES";

        private readonly TurnoPacienteIANegocio negocio =
            new TurnoPacienteIANegocio();

        private readonly OllamaTurnosPacienteService ollama =
            new OllamaTurnosPacienteService();

        private EstadoReservaTurnoIA EstadoActual
        {
            get
            {
                EstadoReservaTurnoIA estado =
                    Session[SESION_ESTADO] as EstadoReservaTurnoIA;

                if (estado == null)
                {
                    estado = new EstadoReservaTurnoIA();
                    Session[SESION_ESTADO] = estado;
                }

                return estado;
            }
            set
            {
                Session[SESION_ESTADO] = value;
            }
        }

        private List<MensajeChatTurnoIA> Mensajes
        {
            get
            {
                List<MensajeChatTurnoIA> mensajes =
                    Session[SESION_MENSAJES] as List<MensajeChatTurnoIA>;

                if (mensajes == null)
                {
                    mensajes = new List<MensajeChatTurnoIA>();
                    Session[SESION_MENSAJES] = mensajes;
                }

                return mensajes;
            }
            set
            {
                Session[SESION_MENSAJES] = value;
            }
        }

        private List<OpcionChatTurnoIA> Opciones
        {
            get
            {
                List<OpcionChatTurnoIA> opciones =
                    Session[SESION_OPCIONES] as List<OpcionChatTurnoIA>;

                if (opciones == null)
                {
                    opciones = new List<OpcionChatTurnoIA>();
                    Session[SESION_OPCIONES] = opciones;
                }

                return opciones;
            }
            set
            {
                Session[SESION_OPCIONES] = value;
            }
        }

        private int IdUsuarioActual
        {
            get
            {
                if (Session["IdUsuario"] == null)
                    return 0;

                int id;
                return int.TryParse(Session["IdUsuario"].ToString(), out id)
                    ? id
                    : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            lblMensaje.Text = "";

            if (IdUsuarioActual <= 0)
            {
                Response.Redirect(ResolveUrl("~/Login.aspx"));
                return;
            }

            if (!IsPostBack)
            {
                InicializarChat();
            }

            ActualizarPantalla();
        }

        private void InicializarChat()
        {
            Paciente paciente =
                new PacienteNegocio().BuscarPorUsuario(IdUsuarioActual);

            lblUsuario.Text =
                "Paciente: " +
                Server.HtmlEncode(
                    Convert.ToString(Session["Nombre"]) +
                    " " +
                    Convert.ToString(Session["Apellido"]));

            Mensajes = new List<MensajeChatTurnoIA>();
            Opciones = new List<OpcionChatTurnoIA>();

            if (paciente == null)
            {
                pnlChat.Enabled = false;

                AgregarMensajeIA(
                    "Tu cuenta todavía no tiene un paciente vinculado. " +
                    "Primero completá tus datos de paciente y después vas a poder gestionar turnos desde acá.");

                return;
            }

            EstadoActual = new EstadoReservaTurnoIA
            {
                IdPaciente = paciente.IdPaciente,
                Paso = "INICIO"
            };

            AgregarMensajeIA(
                "Hola. Desde acá podés sacar un turno, consultar tus próximos turnos o cancelar uno. " +
                "También podés escribir lo que necesitás con tus propias palabras.");

            MostrarMenuPrincipal(false);
        }

        protected async void btnEnviar_Click(object sender, EventArgs e)
        {
            string mensaje = (txtMensaje.Text ?? "").Trim();
            txtMensaje.Text = "";
            lblMensaje.Text = "";

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                lblMensaje.Text = "Escribí un mensaje.";
                ActualizarPantalla();
                return;
            }

            if (mensaje.Length > 500)
            {
                lblMensaje.Text = "El mensaje puede tener como máximo 500 caracteres.";
                ActualizarPantalla();
                return;
            }

            AgregarMensajePaciente(mensaje);

            try
            {
                await ProcesarMensajeLibreAsync(mensaje);
            }
            catch (Exception ex)
            {
                AgregarMensajeIA(
                    "No pude completar la operación. No se realizó ningún cambio en tus turnos.");

                lblMensaje.Text = Server.HtmlEncode(ex.Message);
            }

            ActualizarPantalla();
        }

        protected void rptOpciones_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Elegir")
                return;

            string valor = Convert.ToString(e.CommandArgument);

            try
            {
                ProcesarOpcion(valor);
            }
            catch (Exception ex)
            {
                AgregarMensajeIA(
                    "No pude completar la operación. No se realizó ningún cambio en tus turnos.");

                lblMensaje.Text = Server.HtmlEncode(ex.Message);
            }

            ActualizarPantalla();
        }

        private async Task ProcesarMensajeLibreAsync(string mensaje)
        {
            string normalizado = Normalizar(mensaje);

            if (EsPedidoReinicio(normalizado))
            {
                ReiniciarFlujo("Empezamos de nuevo.");
                return;
            }

            if (EsConsultaClinicaBloqueada(normalizado))
            {
                AgregarMensajeIA(
                    "Puedo ayudarte a sacar, consultar o cancelar turnos, pero no puedo indicar diagnósticos, medicamentos, dosis ni tratamientos. " +
                    "Si querés, puedo ayudarte a buscar un turno con un profesional.");

                MostrarMenuPrincipal(false);
                return;
            }

            if (EsConsultaSobreOtroPaciente(normalizado))
            {
                AgregarMensajeIA(
                    "Por privacidad, desde esta cuenta solo puedo gestionar tus propios turnos y mostrar disponibilidad de profesionales.");

                MostrarMenuPrincipal(false);
                return;
            }

            if (EstadoActual.Paso == "ESPERANDO_MOTIVO")
            {
                GuardarMotivoYConfirmar(mensaje);
                return;
            }

            if (EstadoActual.Paso == "ESPERANDO_CONFIRMACION")
            {
                if (EsConfirmacion(normalizado))
                {
                    ConfirmarReserva();
                    return;
                }

                if (EsNegacionOCambio(normalizado))
                {
                    MostrarHorariosFechaActual(
                        "No hay problema. Elegí otro horario disponible:");
                    return;
                }
            }

            if (EstadoActual.Paso == "ESPERANDO_CONFIRMAR_CANCELACION")
            {
                if (EsConfirmacion(normalizado))
                {
                    ConfirmarCancelacion();
                    return;
                }

                if (EsNegacionOCambio(normalizado))
                {
                    EstadoActual.IdTurnoCancelacion = null;
                    EstadoActual.Paso = "INICIO";
                    AgregarMensajeIA("No cancelé ningún turno.");
                    MostrarMenuPrincipal(false);
                    return;
                }
            }

            if (normalizado.Contains("mis turnos") ||
                normalizado.Contains("que turnos tengo") ||
                normalizado.Contains("proximos turnos"))
            {
                MostrarMisTurnos();
                return;
            }

            if (normalizado.Contains("cancelar turno") ||
                normalizado.Contains("cancelar mi turno") ||
                normalizado.Contains("quiero cancelar"))
            {
                IniciarCancelacion();
                return;
            }

            if (normalizado.Contains("sacar turno") ||
                normalizado.Contains("quiero un turno") ||
                normalizado.Contains("necesito un turno") ||
                normalizado.Contains("reservar turno"))
            {
                await ProcesarNuevaReservaConOllamaAsync(mensaje);
                return;
            }

            if (EstadoActual.Paso == "ESPERANDO_ESPECIALIDAD")
            {
                EspecialidadTurnoIA especialidad =
                    BuscarEspecialidadPorTexto(mensaje);

                if (especialidad != null)
                {
                    SeleccionarEspecialidad(especialidad.IdEspecialidad);
                    return;
                }
            }

            if (EstadoActual.Paso == "ESPERANDO_PROFESIONAL")
            {
                if (normalizado.Contains("cualquiera") ||
                    normalizado.Contains("primero disponible") ||
                    normalizado.Contains("me da igual"))
                {
                    SeleccionarCualquierProfesional();
                    return;
                }

                ProfesionalTurnoIA profesional =
                    BuscarProfesionalPorTexto(
                        mensaje,
                        EstadoActual.IdEspecialidad);

                if (profesional != null)
                {
                    SeleccionarProfesional(profesional.IdProfesional);
                    return;
                }
            }

            if (EstadoActual.Paso == "ESPERANDO_FECHA")
            {
                DateTime fecha;

                if (IntentarResolverFechaTexto(mensaje, out fecha))
                {
                    SeleccionarFecha(fecha);
                    return;
                }
            }

            if (EstadoActual.Paso == "ESPERANDO_HORARIO" &&
                EstadoActual.FechaSeleccionada.HasValue)
            {
                TimeSpan hora;

                if (IntentarResolverHoraTexto(mensaje, out hora))
                {
                    SeleccionarHoraEscrita(hora);
                    return;
                }
            }

            InterpretacionTurnoPacienteIA interpretacion =
                await ollama.InterpretarAsync(
                    mensaje,
                    EstadoActual.Paso);

            if (interpretacion == null)
            {
                AgregarMensajeIA(
                    "No pude interpretar ese mensaje en este momento. " +
                    "Podés seguir usando las opciones de abajo para sacar, consultar o cancelar un turno.");

                RestaurarOpcionesSegunPaso();
                return;
            }

            await EjecutarInterpretacionAsync(
                interpretacion,
                mensaje);
        }

        private async Task ProcesarNuevaReservaConOllamaAsync(
            string mensaje)
        {
            InterpretacionTurnoPacienteIA interpretacion =
                await ollama.InterpretarAsync(
                    mensaje,
                    "INICIO");

            if (interpretacion == null)
            {
                IniciarNuevaReserva();
                return;
            }

            if (interpretacion.Accion == "CONSULTA_CLINICA")
            {
                AgregarMensajeIA(
                    "Puedo ayudarte a gestionar el turno, pero no a diagnosticar ni indicar tratamientos. " +
                    "Empecemos por elegir la especialidad.");

                IniciarNuevaReserva();
                return;
            }

            if (interpretacion.Accion != "NUEVO_TURNO")
                interpretacion.Accion = "NUEVO_TURNO";

            await EjecutarInterpretacionAsync(
                interpretacion,
                mensaje);
        }

        private Task EjecutarInterpretacionAsync(
            InterpretacionTurnoPacienteIA interpretacion,
            string mensajeOriginal)
        {
            switch (interpretacion.Accion)
            {
                case "MIS_TURNOS":
                    MostrarMisTurnos();
                    break;

                case "CANCELAR_TURNO":
                    IniciarCancelacion();
                    break;

                case "REINICIAR":
                    ReiniciarFlujo("Empezamos de nuevo.");
                    break;

                case "CONFIRMAR":
                    if (EstadoActual.Paso == "ESPERANDO_CONFIRMACION")
                        ConfirmarReserva();
                    else if (EstadoActual.Paso == "ESPERANDO_CONFIRMAR_CANCELACION")
                        ConfirmarCancelacion();
                    else
                    {
                        AgregarMensajeIA(
                            "Todavía no hay una operación pendiente para confirmar.");
                        MostrarMenuPrincipal(false);
                    }
                    break;

                case "CAMBIAR_HORARIO":
                    if (EstadoActual.FechaSeleccionada.HasValue)
                        MostrarHorariosFechaActual("Elegí otro horario:");
                    else
                        MostrarFechasDisponibles();
                    break;

                case "CONSULTA_CLINICA":
                    AgregarMensajeIA(
                        "Puedo ayudarte con turnos, pero no con diagnósticos, medicamentos, dosis o tratamientos. " +
                        "Si necesitás atención, puedo ayudarte a buscar un turno.");
                    MostrarMenuPrincipal(false);
                    break;

                case "NUEVO_TURNO":
                    ProcesarDatosNuevaReserva(
                        interpretacion,
                        mensajeOriginal);
                    break;

                case "AYUDA":
                    AgregarMensajeIA(
                        "Podés pedirme cosas como: “quiero sacar un turno”, “qué turnos tengo”, “cancelar mi turno”, " +
                        "“necesito cardiología la semana que viene” o “quiero un horario después de las 15”.");
                    MostrarMenuPrincipal(false);
                    break;

                default:
                    AgregarMensajeIA(
                        "Puedo ayudarte a sacar, consultar o cancelar tus turnos. Elegí una opción o decime qué necesitás.");
                    MostrarMenuPrincipal(false);
                    break;
            }

            return Task.CompletedTask;
        }

        private void ProcesarDatosNuevaReserva(
            InterpretacionTurnoPacienteIA interpretacion,
            string mensajeOriginal)
        {
            EstadoReservaTurnoIA estado = new EstadoReservaTurnoIA
            {
                IdPaciente = EstadoActual.IdPaciente,
                Paso = "INICIO"
            };

            EstadoActual = estado;

            AplicarPreferenciaHoraria(interpretacion);

            EspecialidadTurnoIA especialidad = null;
            ProfesionalTurnoIA profesional = null;

            if (!string.IsNullOrWhiteSpace(interpretacion.Profesional))
            {
                profesional = BuscarProfesionalPorTexto(
                    interpretacion.Profesional,
                    null);

                if (profesional != null)
                {
                    estado.IdProfesional = profesional.IdProfesional;
                    estado.Profesional = profesional.NombreCompleto;
                    estado.IdEspecialidad = profesional.IdEspecialidad;
                    estado.Especialidad = profesional.Especialidad;
                }
            }

            if (!estado.IdEspecialidad.HasValue &&
                !string.IsNullOrWhiteSpace(interpretacion.Especialidad))
            {
                especialidad = BuscarEspecialidadPorTexto(
                    interpretacion.Especialidad);

                if (especialidad != null)
                {
                    estado.IdEspecialidad = especialidad.IdEspecialidad;
                    estado.Especialidad = especialidad.Nombre;
                }
            }

            string normalizado = Normalizar(mensajeOriginal);

            if (normalizado.Contains("cualquiera") ||
                normalizado.Contains("primero disponible") ||
                normalizado.Contains("me da igual el medico") ||
                normalizado.Contains("me da igual el profesional"))
            {
                estado.CualquierProfesional = true;
                estado.IdProfesional = null;
                estado.Profesional = null;
            }

            DateTime desde;
            DateTime hasta;
            bool tienePeriodo = ResolverRangoInterpretacion(
                interpretacion,
                mensajeOriginal,
                out desde,
                out hasta);

            if (!estado.IdEspecialidad.HasValue &&
                !estado.IdProfesional.HasValue)
            {
                MostrarEspecialidades(
                    "Primero elegí la especialidad para la que querés reservar:");
                return;
            }

            if (estado.IdProfesional.HasValue && tienePeriodo)
            {
                MostrarSlotsEnRango(
                    desde,
                    hasta,
                    "Estos son los primeros horarios disponibles que encontré:");
                return;
            }

            if (estado.CualquierProfesional &&
                estado.IdEspecialidad.HasValue &&
                tienePeriodo)
            {
                MostrarSlotsEnRango(
                    desde,
                    hasta,
                    "Estos son los primeros horarios disponibles de la especialidad:");
                return;
            }

            if (estado.IdProfesional.HasValue)
            {
                MostrarFechasDisponibles();
                return;
            }

            MostrarProfesionales(
                "Elegí con qué profesional querés atenderte. También podés pedir el primero disponible:");
        }

        private void ProcesarOpcion(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return;

            if (valor == "NUEVO_TURNO")
            {
                AgregarMensajePaciente("Sacar turno");
                IniciarNuevaReserva();
                return;
            }

            if (valor == "MIS_TURNOS")
            {
                AgregarMensajePaciente("Mis próximos turnos");
                MostrarMisTurnos();
                return;
            }

            if (valor == "CANCELAR")
            {
                AgregarMensajePaciente("Cancelar un turno");
                IniciarCancelacion();
                return;
            }

            if (valor == "REINICIAR")
            {
                AgregarMensajePaciente("Empezar de nuevo");
                ReiniciarFlujo("Empezamos de nuevo.");
                return;
            }

            if (valor == "OMITIR_MOTIVO")
            {
                AgregarMensajePaciente("Omitir motivo");
                EstadoActual.Motivo = null;
                MostrarConfirmacionReserva();
                return;
            }

            if (valor == "CONFIRMAR_RESERVA")
            {
                AgregarMensajePaciente("Confirmar turno");
                ConfirmarReserva();
                return;
            }

            if (valor == "CAMBIAR_HORARIO")
            {
                AgregarMensajePaciente("Elegir otro horario");
                MostrarHorariosFechaActual("Elegí otro horario disponible:");
                return;
            }

            if (valor == "NO_CANCELAR")
            {
                AgregarMensajePaciente("No cancelar");
                EstadoActual.IdTurnoCancelacion = null;
                EstadoActual.Paso = "INICIO";
                AgregarMensajeIA("No cancelé ningún turno.");
                MostrarMenuPrincipal(false);
                return;
            }

            string[] partes = valor.Split('|');

            if (partes.Length == 0)
                return;

            switch (partes[0])
            {
                case "ESPECIALIDAD":
                    if (partes.Length >= 2)
                    {
                        int idEspecialidad;
                        if (int.TryParse(partes[1], out idEspecialidad))
                        {
                            AgregarMensajePaciente(
                                ObtenerNombreEspecialidad(idEspecialidad));
                            SeleccionarEspecialidad(idEspecialidad);
                        }
                    }
                    break;

                case "PROFESIONAL":
                    if (partes.Length >= 2)
                    {
                        int idProfesional;
                        if (int.TryParse(partes[1], out idProfesional))
                        {
                            ProfesionalTurnoIA profesional =
                                negocio.ListarProfesionalesDisponibles(
                                    EstadoActual.IdEspecialidad)
                                    .FirstOrDefault(p => p.IdProfesional == idProfesional);

                            if (profesional != null)
                                AgregarMensajePaciente(profesional.NombreCompleto);

                            SeleccionarProfesional(idProfesional);
                        }
                    }
                    break;

                case "CUALQUIERA":
                    AgregarMensajePaciente("El primero disponible");
                    SeleccionarCualquierProfesional();
                    break;

                case "FECHA":
                    if (partes.Length >= 2)
                    {
                        DateTime fecha;
                        if (DateTime.TryParseExact(
                            partes[1],
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out fecha))
                        {
                            AgregarMensajePaciente(FormatearFecha(fecha));
                            SeleccionarFecha(fecha);
                        }
                    }
                    break;

                case "SLOT":
                    if (partes.Length >= 4)
                    {
                        int idProfesional;
                        int duracion;
                        DateTime fechaHora;

                        if (int.TryParse(partes[1], out idProfesional) &&
                            DateTime.TryParseExact(
                                partes[2],
                                "yyyyMMddHHmm",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out fechaHora) &&
                            int.TryParse(partes[3], out duracion))
                        {
                            AgregarMensajePaciente(
                                FormatearFecha(fechaHora) +
                                " a las " +
                                fechaHora.ToString("HH:mm"));

                            SeleccionarSlot(
                                idProfesional,
                                fechaHora,
                                duracion);
                        }
                    }
                    break;

                case "CANCELAR_TURNO":
                    if (partes.Length >= 2)
                    {
                        int idTurno;
                        if (int.TryParse(partes[1], out idTurno))
                            PrepararCancelacion(idTurno);
                    }
                    break;

                case "CONFIRMAR_CANCELACION":
                    if (partes.Length >= 2)
                    {
                        int idTurno;
                        if (int.TryParse(partes[1], out idTurno))
                        {
                            EstadoActual.IdTurnoCancelacion = idTurno;
                            AgregarMensajePaciente("Sí, cancelar turno");
                            ConfirmarCancelacion();
                        }
                    }
                    break;
            }
        }

        private void IniciarNuevaReserva()
        {
            EstadoActual = new EstadoReservaTurnoIA
            {
                IdPaciente = EstadoActual.IdPaciente,
                Paso = "ESPERANDO_ESPECIALIDAD"
            };

            MostrarEspecialidades(
                "¿Con qué especialidad necesitás el turno?");
        }

        private void MostrarEspecialidades(string mensaje)
        {
            List<EspecialidadTurnoIA> especialidades =
                negocio.ListarEspecialidadesDisponibles();

            EstadoActual.Paso = "ESPERANDO_ESPECIALIDAD";

            if (especialidades.Count == 0)
            {
                AgregarMensajeIA(
                    "En este momento no hay especialidades habilitadas para reserva online.");
                MostrarMenuPrincipal(false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
                AgregarMensajeIA(mensaje);

            DefinirOpciones(
                especialidades
                    .Select(e => new OpcionChatTurnoIA(
                        e.Nombre,
                        "ESPECIALIDAD|" + e.IdEspecialidad))
                    .ToList());
        }

        private void SeleccionarEspecialidad(int idEspecialidad)
        {
            EspecialidadTurnoIA especialidad =
                negocio.ListarEspecialidadesDisponibles()
                    .FirstOrDefault(e => e.IdEspecialidad == idEspecialidad);

            if (especialidad == null)
            {
                MostrarEspecialidades(
                    "Esa especialidad ya no está disponible para reserva online. Elegí otra:");
                return;
            }

            EstadoActual.IdEspecialidad = especialidad.IdEspecialidad;
            EstadoActual.Especialidad = especialidad.Nombre;
            EstadoActual.IdProfesional = null;
            EstadoActual.Profesional = null;
            EstadoActual.CualquierProfesional = false;
            EstadoActual.FechaSeleccionada = null;
            EstadoActual.FechaHoraSeleccionada = null;
            EstadoActual.Motivo = null;

            MostrarProfesionales(
                "Encontré estos profesionales de " + especialidad.Nombre + ". ¿Con cuál preferís atenderte?");
        }

        private void MostrarProfesionales(string mensaje)
        {
            if (!EstadoActual.IdEspecialidad.HasValue)
            {
                MostrarEspecialidades(
                    "Primero elegí una especialidad:");
                return;
            }

            List<ProfesionalTurnoIA> profesionales =
                negocio.ListarProfesionalesDisponibles(
                    EstadoActual.IdEspecialidad);

            EstadoActual.Paso = "ESPERANDO_PROFESIONAL";

            if (profesionales.Count == 0)
            {
                AgregarMensajeIA(
                    "No encontré profesionales con agenda activa para esa especialidad.");
                MostrarEspecialidades("Podés elegir otra especialidad:");
                return;
            }

            AgregarMensajeIA(mensaje);

            List<OpcionChatTurnoIA> opciones = profesionales
                .Take(8)
                .Select(p => new OpcionChatTurnoIA(
                    p.NombreCompleto,
                    "PROFESIONAL|" + p.IdProfesional))
                .ToList();

            opciones.Add(new OpcionChatTurnoIA(
                "El primero disponible",
                "CUALQUIERA"));

            DefinirOpciones(opciones);
        }

        private void SeleccionarProfesional(int idProfesional)
        {
            ProfesionalTurnoIA profesional =
                negocio.ListarProfesionalesDisponibles(
                    EstadoActual.IdEspecialidad)
                    .FirstOrDefault(p => p.IdProfesional == idProfesional);

            if (profesional == null)
            {
                MostrarProfesionales(
                    "Ese profesional ya no está disponible. Elegí otra opción:");
                return;
            }

            EstadoActual.IdProfesional = profesional.IdProfesional;
            EstadoActual.Profesional = profesional.NombreCompleto;
            EstadoActual.IdEspecialidad = profesional.IdEspecialidad;
            EstadoActual.Especialidad = profesional.Especialidad;
            EstadoActual.CualquierProfesional = false;
            EstadoActual.FechaSeleccionada = null;
            EstadoActual.FechaHoraSeleccionada = null;

            MostrarFechasDisponibles();
        }

        private void SeleccionarCualquierProfesional()
        {
            EstadoActual.IdProfesional = null;
            EstadoActual.Profesional = null;
            EstadoActual.CualquierProfesional = true;
            EstadoActual.FechaSeleccionada = null;
            EstadoActual.FechaHoraSeleccionada = null;

            MostrarFechasDisponibles();
        }

        private void MostrarFechasDisponibles()
        {
            DateTime desde = DateTime.Today;
            DateTime hasta = desde.AddDays(30);

            List<DateTime> fechas = negocio.ObtenerFechasConDisponibilidad(
                EstadoActual.IdPaciente,
                EstadoActual.IdEspecialidad,
                EstadoActual.IdProfesional,
                desde,
                hasta,
                EstadoActual.HoraDesde,
                EstadoActual.HoraHasta,
                6);

            EstadoActual.Paso = "ESPERANDO_FECHA";

            if (fechas.Count == 0)
            {
                AgregarMensajeIA(
                    "No encontré horarios libres durante los próximos 30 días con esa selección.");

                if (EstadoActual.IdEspecialidad.HasValue)
                    MostrarProfesionales(
                        "Podés probar con otro profesional:");
                else
                    MostrarMenuPrincipal(false);

                return;
            }

            string quien = EstadoActual.CualquierProfesional
                ? "la especialidad"
                : EstadoActual.Profesional;

            AgregarMensajeIA(
                "Estos son los próximos días con disponibilidad para " +
                quien +
                ". Elegí uno:");

            DefinirOpciones(
                fechas.Select(f => new OpcionChatTurnoIA(
                    FormatearFechaCorta(f),
                    "FECHA|" + f.ToString("yyyy-MM-dd")))
                    .ToList());
        }

        private void SeleccionarFecha(DateTime fecha)
        {
            if (fecha.Date < DateTime.Today)
            {
                AgregarMensajeIA("No puedo reservar un turno en una fecha pasada.");
                MostrarFechasDisponibles();
                return;
            }

            EstadoActual.FechaSeleccionada = fecha.Date;
            EstadoActual.FechaHoraSeleccionada = null;

            MostrarHorariosFechaActual(
                "Para el " + FormatearFecha(fecha) + " están disponibles estos horarios:");
        }

        private void MostrarHorariosFechaActual(string mensaje)
        {
            if (!EstadoActual.FechaSeleccionada.HasValue)
            {
                MostrarFechasDisponibles();
                return;
            }

            DateTime fecha = EstadoActual.FechaSeleccionada.Value.Date;

            List<SlotTurnoIA> slots = negocio.ObtenerSlotsDisponibles(
                EstadoActual.IdPaciente,
                EstadoActual.IdEspecialidad,
                EstadoActual.IdProfesional,
                fecha,
                fecha,
                EstadoActual.HoraDesde,
                EstadoActual.HoraHasta,
                10);

            EstadoActual.Paso = "ESPERANDO_HORARIO";

            if (slots.Count == 0)
            {
                AgregarMensajeIA(
                    "Ese día ya no tiene horarios disponibles con tu selección.");
                MostrarFechasDisponibles();
                return;
            }

            AgregarMensajeIA(mensaje);
            MostrarOpcionesSlots(slots, false);
        }

        private void MostrarSlotsEnRango(
            DateTime desde,
            DateTime hasta,
            string mensaje)
        {
            List<SlotTurnoIA> slots = negocio.ObtenerSlotsDisponibles(
                EstadoActual.IdPaciente,
                EstadoActual.IdEspecialidad,
                EstadoActual.IdProfesional,
                desde,
                hasta,
                EstadoActual.HoraDesde,
                EstadoActual.HoraHasta,
                8);

            EstadoActual.Paso = "ESPERANDO_HORARIO";

            if (slots.Count == 0)
            {
                AgregarMensajeIA(
                    "No encontré horarios libres para ese período. Te muestro las próximas fechas disponibles.");
                MostrarFechasDisponibles();
                return;
            }

            AgregarMensajeIA(mensaje);
            MostrarOpcionesSlots(slots, true);
        }

        private void MostrarOpcionesSlots(
            List<SlotTurnoIA> slots,
            bool incluirFecha)
        {
            List<OpcionChatTurnoIA> opciones =
                new List<OpcionChatTurnoIA>();

            foreach (SlotTurnoIA slot in slots)
            {
                string texto = incluirFecha
                    ? FormatearFechaCorta(slot.FechaHora) +
                      " · " + slot.FechaHora.ToString("HH:mm")
                    : slot.FechaHora.ToString("HH:mm");

                if (EstadoActual.CualquierProfesional ||
                    !EstadoActual.IdProfesional.HasValue)
                {
                    texto += " · " + slot.Profesional;
                }

                opciones.Add(new OpcionChatTurnoIA(
                    texto,
                    "SLOT|" +
                    slot.IdProfesional + "|" +
                    slot.FechaHora.ToString("yyyyMMddHHmm") + "|" +
                    slot.DuracionMinutos));
            }

            DefinirOpciones(opciones);
        }

        private void SeleccionarHoraEscrita(TimeSpan hora)
        {
            DateTime fecha = EstadoActual.FechaSeleccionada.Value.Date;

            List<SlotTurnoIA> slots = negocio.ObtenerSlotsDisponibles(
                EstadoActual.IdPaciente,
                EstadoActual.IdEspecialidad,
                EstadoActual.IdProfesional,
                fecha,
                fecha,
                EstadoActual.HoraDesde,
                EstadoActual.HoraHasta,
                100);

            List<SlotTurnoIA> coincidencias = slots
                .Where(s => s.FechaHora.TimeOfDay == hora)
                .ToList();

            if (coincidencias.Count == 1)
            {
                SlotTurnoIA slot = coincidencias[0];
                SeleccionarSlot(
                    slot.IdProfesional,
                    slot.FechaHora,
                    slot.DuracionMinutos);
                return;
            }

            if (coincidencias.Count > 1)
            {
                AgregarMensajeIA(
                    "Ese horario está disponible con más de un profesional. Elegí una opción:");
                MostrarOpcionesSlots(coincidencias, false);
                return;
            }

            AgregarMensajeIA(
                "Ese horario no está disponible. Elegí uno de los horarios libres:");
            MostrarOpcionesSlots(slots.Take(10).ToList(), false);
        }

        private void SeleccionarSlot(
            int idProfesional,
            DateTime fechaHora,
            int duracion)
        {
            List<SlotTurnoIA> slotsReales = negocio.ObtenerSlotsDisponibles(
                EstadoActual.IdPaciente,
                EstadoActual.IdEspecialidad,
                idProfesional,
                fechaHora.Date,
                fechaHora.Date,
                null,
                null,
                100);

            SlotTurnoIA slot = slotsReales.FirstOrDefault(s =>
                s.IdProfesional == idProfesional &&
                s.FechaHora == fechaHora &&
                s.DuracionMinutos == duracion);

            if (slot == null)
            {
                EstadoActual.FechaSeleccionada = fechaHora.Date;
                AgregarMensajeIA(
                    "Ese horario acaba de dejar de estar disponible. Te muestro las opciones actualizadas.");
                MostrarHorariosFechaActual(null);
                return;
            }

            EstadoActual.IdProfesional = slot.IdProfesional;
            EstadoActual.Profesional = slot.Profesional;
            EstadoActual.IdEspecialidad = slot.IdEspecialidad;
            EstadoActual.Especialidad = slot.Especialidad;
            EstadoActual.FechaSeleccionada = slot.FechaHora.Date;
            EstadoActual.FechaHoraSeleccionada = slot.FechaHora;
            EstadoActual.DuracionMinutos = slot.DuracionMinutos;
            EstadoActual.Paso = "ESPERANDO_MOTIVO";

            AgregarMensajeIA(
                "Perfecto. ¿Querés indicar brevemente el motivo de la consulta? " +
                "Es opcional y puede tener hasta 300 caracteres.");

            DefinirOpciones(new List<OpcionChatTurnoIA>
            {
                new OpcionChatTurnoIA("Omitir motivo", "OMITIR_MOTIVO"),
                new OpcionChatTurnoIA("Elegir otro horario", "CAMBIAR_HORARIO")
            });
        }

        private void GuardarMotivoYConfirmar(string motivo)
        {
            if (Normalizar(motivo).Contains("omitir"))
                EstadoActual.Motivo = null;
            else
            {
                string limpio = motivo.Trim();

                if (limpio.Length > 300)
                    limpio = limpio.Substring(0, 300);

                EstadoActual.Motivo = limpio;
            }

            MostrarConfirmacionReserva();
        }

        private void MostrarConfirmacionReserva()
        {
            if (!EstadoActual.FechaHoraSeleccionada.HasValue ||
                !EstadoActual.IdProfesional.HasValue)
            {
                AgregarMensajeIA(
                    "Falta elegir un horario antes de confirmar.");
                MostrarFechasDisponibles();
                return;
            }

            EstadoActual.Paso = "ESPERANDO_CONFIRMACION";

            string motivo = string.IsNullOrWhiteSpace(EstadoActual.Motivo)
                ? "Sin motivo informado"
                : EstadoActual.Motivo;

            StringBuilder texto = new StringBuilder();
            texto.AppendLine("Revisá los datos antes de confirmar:");
            texto.AppendLine();
            texto.AppendLine("Especialidad: " + EstadoActual.Especialidad);
            texto.AppendLine("Profesional: " + EstadoActual.Profesional);
            texto.AppendLine("Fecha: " + FormatearFecha(EstadoActual.FechaHoraSeleccionada.Value));
            texto.AppendLine("Hora: " + EstadoActual.FechaHoraSeleccionada.Value.ToString("HH:mm"));
            texto.AppendLine("Duración: " + EstadoActual.DuracionMinutos + " minutos");
            texto.Append("Motivo: " + motivo);

            AgregarMensajeIA(texto.ToString());

            DefinirOpciones(new List<OpcionChatTurnoIA>
            {
                new OpcionChatTurnoIA("Confirmar turno", "CONFIRMAR_RESERVA"),
                new OpcionChatTurnoIA("Elegir otro horario", "CAMBIAR_HORARIO"),
                new OpcionChatTurnoIA("Empezar de nuevo", "REINICIAR")
            });
        }

        private void ConfirmarReserva()
        {
            if (!EstadoActual.FechaHoraSeleccionada.HasValue ||
                !EstadoActual.IdProfesional.HasValue)
            {
                AgregarMensajeIA(
                    "No hay un turno completo pendiente de confirmación.");
                MostrarMenuPrincipal(false);
                return;
            }

            ResultadoReservaTurnoIA resultado = negocio.ReservarTurno(
                IdUsuarioActual,
                EstadoActual.IdPaciente,
                EstadoActual.IdProfesional.Value,
                EstadoActual.FechaHoraSeleccionada.Value,
                EstadoActual.DuracionMinutos,
                EstadoActual.Motivo);

            if (!resultado.Exito)
            {
                AgregarMensajeIA(resultado.Mensaje);

                DateTime fecha = EstadoActual.FechaHoraSeleccionada.Value.Date;
                EstadoActual.FechaSeleccionada = fecha;
                EstadoActual.FechaHoraSeleccionada = null;
                EstadoActual.Motivo = null;

                MostrarHorariosFechaActual(
                    "Te muestro los horarios que siguen disponibles:");
                return;
            }

            TurnoPacienteResumenIA turno = resultado.Turno;

            StringBuilder texto = new StringBuilder();
            texto.AppendLine("Turno reservado correctamente.");

            if (turno != null)
            {
                texto.AppendLine();
                texto.AppendLine(FormatearFecha(turno.FechaHora) + " a las " + turno.FechaHora.ToString("HH:mm"));
                texto.AppendLine(turno.Profesional + " — " + turno.Especialidad);
                texto.Append("Estado: " + turno.Estado);
            }
            else
            {
                texto.Append("Número de turno: " + resultado.IdTurno);
            }

            AgregarMensajeIA(texto.ToString());

            int idPaciente = EstadoActual.IdPaciente;
            EstadoActual = new EstadoReservaTurnoIA
            {
                IdPaciente = idPaciente,
                Paso = "INICIO"
            };

            MostrarMenuPrincipal(false);
        }

        private void MostrarMisTurnos()
        {
            List<TurnoPacienteResumenIA> turnos =
                negocio.ListarProximosTurnosPaciente(
                    IdUsuarioActual,
                    EstadoActual.IdPaciente);

            EstadoActual.Paso = "INICIO";

            if (turnos.Count == 0)
            {
                AgregarMensajeIA(
                    "No tenés próximos turnos activos.");
                MostrarMenuPrincipal(false);
                return;
            }

            StringBuilder texto = new StringBuilder();
            texto.AppendLine(
                turnos.Count == 1
                    ? "Tenés 1 próximo turno:"
                    : "Tenés " + turnos.Count + " próximos turnos:");

            foreach (TurnoPacienteResumenIA turno in turnos.Take(10))
            {
                texto.AppendLine();
                texto.AppendLine(
                    FormatearFecha(turno.FechaHora) +
                    " · " + turno.FechaHora.ToString("HH:mm"));
                texto.AppendLine(turno.Profesional + " — " + turno.Especialidad);
                texto.Append("Estado: " + turno.Estado);
            }

            AgregarMensajeIA(texto.ToString());
            MostrarMenuPrincipal(false);
        }

        private void IniciarCancelacion()
        {
            List<TurnoPacienteResumenIA> turnos =
                negocio.ListarProximosTurnosPaciente(
                    IdUsuarioActual,
                    EstadoActual.IdPaciente);

            if (turnos.Count == 0)
            {
                AgregarMensajeIA(
                    "No tenés próximos turnos activos para cancelar.");
                MostrarMenuPrincipal(false);
                return;
            }

            EstadoActual.Paso = "ESPERANDO_TURNO_CANCELAR";

            AgregarMensajeIA(
                "Elegí el turno que querés cancelar:");

            List<OpcionChatTurnoIA> opciones = turnos
                .Take(8)
                .Select(t => new OpcionChatTurnoIA(
                    FormatearFechaCorta(t.FechaHora) +
                    " · " + t.FechaHora.ToString("HH:mm") +
                    " · " + t.Profesional,
                    "CANCELAR_TURNO|" + t.IdTurno))
                .ToList();

            opciones.Add(new OpcionChatTurnoIA(
                "Volver",
                "REINICIAR"));

            DefinirOpciones(opciones);
        }

        private void PrepararCancelacion(int idTurno)
        {
            TurnoPacienteResumenIA turno =
                negocio.BuscarTurnoPaciente(
                    IdUsuarioActual,
                    EstadoActual.IdPaciente,
                    idTurno);

            if (turno == null ||
                turno.FechaHora < DateTime.Now ||
                string.Equals(
                    turno.Estado,
                    "Cancelado",
                    StringComparison.OrdinalIgnoreCase))
            {
                AgregarMensajeIA(
                    "Ese turno ya no está disponible para cancelar.");
                IniciarCancelacion();
                return;
            }

            EstadoActual.IdTurnoCancelacion = idTurno;
            EstadoActual.Paso = "ESPERANDO_CONFIRMAR_CANCELACION";

            AgregarMensajeIA(
                "¿Confirmás que querés cancelar el turno del " +
                FormatearFecha(turno.FechaHora) +
                " a las " +
                turno.FechaHora.ToString("HH:mm") +
                " con " +
                turno.Profesional +
                "?");

            DefinirOpciones(new List<OpcionChatTurnoIA>
            {
                new OpcionChatTurnoIA(
                    "Sí, cancelar turno",
                    "CONFIRMAR_CANCELACION|" + idTurno),
                new OpcionChatTurnoIA(
                    "No",
                    "NO_CANCELAR")
            });
        }

        private void ConfirmarCancelacion()
        {
            if (!EstadoActual.IdTurnoCancelacion.HasValue)
            {
                AgregarMensajeIA(
                    "No hay un turno pendiente de cancelación.");
                MostrarMenuPrincipal(false);
                return;
            }

            ResultadoOperacionTurnoIA resultado =
                negocio.CancelarTurnoPaciente(
                    IdUsuarioActual,
                    EstadoActual.IdPaciente,
                    EstadoActual.IdTurnoCancelacion.Value);

            AgregarMensajeIA(resultado.Mensaje);

            int idPaciente = EstadoActual.IdPaciente;
            EstadoActual = new EstadoReservaTurnoIA
            {
                IdPaciente = idPaciente,
                Paso = "INICIO"
            };

            MostrarMenuPrincipal(false);
        }

        private void MostrarMenuPrincipal(bool agregarMensaje)
        {
            EstadoActual.Paso = "INICIO";

            if (agregarMensaje)
            {
                AgregarMensajeIA(
                    "¿Qué querés hacer?");
            }

            DefinirOpciones(new List<OpcionChatTurnoIA>
            {
                new OpcionChatTurnoIA("Sacar turno", "NUEVO_TURNO"),
                new OpcionChatTurnoIA("Mis próximos turnos", "MIS_TURNOS"),
                new OpcionChatTurnoIA("Cancelar un turno", "CANCELAR")
            });
        }

        private void ReiniciarFlujo(string mensaje)
        {
            int idPaciente = EstadoActual.IdPaciente;

            EstadoActual = new EstadoReservaTurnoIA
            {
                IdPaciente = idPaciente,
                Paso = "INICIO"
            };

            if (!string.IsNullOrWhiteSpace(mensaje))
                AgregarMensajeIA(mensaje);

            MostrarMenuPrincipal(false);
        }

        private void RestaurarOpcionesSegunPaso()
        {
            switch (EstadoActual.Paso)
            {
                case "ESPERANDO_ESPECIALIDAD":
                    MostrarEspecialidades(null);
                    break;

                case "ESPERANDO_PROFESIONAL":
                    MostrarProfesionales(
                        "Elegí un profesional o el primero disponible:");
                    break;

                case "ESPERANDO_FECHA":
                    MostrarFechasDisponibles();
                    break;

                case "ESPERANDO_HORARIO":
                    if (EstadoActual.FechaSeleccionada.HasValue)
                        MostrarHorariosFechaActual(
                            "Elegí uno de los horarios disponibles:");
                    else
                        MostrarFechasDisponibles();
                    break;

                case "ESPERANDO_MOTIVO":
                    DefinirOpciones(new List<OpcionChatTurnoIA>
                    {
                        new OpcionChatTurnoIA("Omitir motivo", "OMITIR_MOTIVO"),
                        new OpcionChatTurnoIA("Elegir otro horario", "CAMBIAR_HORARIO")
                    });
                    break;

                case "ESPERANDO_CONFIRMACION":
                    DefinirOpciones(new List<OpcionChatTurnoIA>
                    {
                        new OpcionChatTurnoIA("Confirmar turno", "CONFIRMAR_RESERVA"),
                        new OpcionChatTurnoIA("Elegir otro horario", "CAMBIAR_HORARIO"),
                        new OpcionChatTurnoIA("Empezar de nuevo", "REINICIAR")
                    });
                    break;

                case "ESPERANDO_TURNO_CANCELAR":
                    IniciarCancelacion();
                    break;

                default:
                    MostrarMenuPrincipal(false);
                    break;
            }
        }

        private EspecialidadTurnoIA BuscarEspecialidadPorTexto(string texto)
        {
            string buscado = Normalizar(texto);

            List<EspecialidadTurnoIA> especialidades =
                negocio.ListarEspecialidadesDisponibles();

            return especialidades.FirstOrDefault(e =>
                Normalizar(e.Nombre) == buscado ||
                buscado.Contains(Normalizar(e.Nombre)) ||
                Normalizar(e.Nombre).Contains(buscado));
        }

        private ProfesionalTurnoIA BuscarProfesionalPorTexto(
            string texto,
            int? idEspecialidad)
        {
            string buscado = Normalizar(texto);

            List<ProfesionalTurnoIA> profesionales =
                negocio.ListarProfesionalesDisponibles(idEspecialidad);

            return profesionales.FirstOrDefault(p =>
                buscado.Contains(Normalizar(p.Apellido)) ||
                buscado.Contains(Normalizar(p.NombreCompleto)) ||
                Normalizar(p.NombreCompleto).Contains(buscado));
        }

        private string ObtenerNombreEspecialidad(int idEspecialidad)
        {
            EspecialidadTurnoIA especialidad =
                negocio.ListarEspecialidadesDisponibles()
                    .FirstOrDefault(e => e.IdEspecialidad == idEspecialidad);

            return especialidad == null
                ? "Especialidad"
                : especialidad.Nombre;
        }

        private void AplicarPreferenciaHoraria(
            InterpretacionTurnoPacienteIA interpretacion)
        {
            EstadoActual.PreferenciaMomento = interpretacion.Momento;
            EstadoActual.HoraDesde = null;
            EstadoActual.HoraHasta = null;

            TimeSpan hora;

            if (TimeSpan.TryParse(interpretacion.HoraDesde, out hora))
                EstadoActual.HoraDesde = hora;

            if (TimeSpan.TryParse(interpretacion.HoraHasta, out hora))
                EstadoActual.HoraHasta = hora;

            if (!EstadoActual.HoraDesde.HasValue &&
                !EstadoActual.HoraHasta.HasValue)
            {
                if (interpretacion.Momento == "MANANA")
                {
                    EstadoActual.HoraHasta = new TimeSpan(13, 0, 0);
                }
                else if (interpretacion.Momento == "TARDE")
                {
                    EstadoActual.HoraDesde = new TimeSpan(13, 0, 0);
                }
            }
        }

        private bool ResolverRangoInterpretacion(
            InterpretacionTurnoPacienteIA interpretacion,
            string mensajeOriginal,
            out DateTime desde,
            out DateTime hasta)
        {
            desde = DateTime.Today;
            hasta = DateTime.Today.AddDays(30);

            switch (interpretacion.Periodo)
            {
                case "HOY":
                    desde = DateTime.Today;
                    hasta = DateTime.Today;
                    return true;

                case "MANANA":
                    desde = DateTime.Today.AddDays(1);
                    hasta = desde;
                    return true;

                case "PASADO_MANANA":
                    desde = DateTime.Today.AddDays(2);
                    hasta = desde;
                    return true;

                case "ESTA_SEMANA":
                    desde = DateTime.Today;
                    hasta = ObtenerLunesSemana(DateTime.Today).AddDays(6);
                    if (hasta < desde)
                        hasta = desde;
                    return true;

                case "PROXIMA_SEMANA":
                    desde = ObtenerLunesSemana(DateTime.Today).AddDays(7);
                    hasta = desde.AddDays(6);
                    return true;

                case "ESTE_MES":
                    desde = DateTime.Today;
                    hasta = new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1)
                        .AddMonths(1)
                        .AddDays(-1);
                    return true;

                case "FECHA_EXPLICITA":
                    DateTime fecha;
                    string fuente = string.IsNullOrWhiteSpace(interpretacion.Fecha)
                        ? mensajeOriginal
                        : interpretacion.Fecha;

                    if (IntentarResolverFechaTexto(fuente, out fecha))
                    {
                        desde = fecha.Date;
                        hasta = fecha.Date;
                        return true;
                    }
                    return false;

                default:
                    DateTime fechaTexto;
                    if (IntentarResolverFechaTexto(mensajeOriginal, out fechaTexto))
                    {
                        desde = fechaTexto.Date;
                        hasta = fechaTexto.Date;
                        return true;
                    }
                    return false;
            }
        }

        private bool IntentarResolverFechaTexto(
            string texto,
            out DateTime fecha)
        {
            fecha = DateTime.MinValue;
            string normalizado = Normalizar(texto);

            if (normalizado.Contains("pasado manana"))
            {
                fecha = DateTime.Today.AddDays(2);
                return true;
            }

            if (normalizado.Contains("manana"))
            {
                fecha = DateTime.Today.AddDays(1);
                return true;
            }

            if (Regex.IsMatch(normalizado, @"\bhoy\b"))
            {
                fecha = DateTime.Today;
                return true;
            }

            Match numerica = Regex.Match(
                normalizado,
                @"\b(\d{1,2})/(\d{1,2})(?:/(\d{2,4}))?\b");

            if (numerica.Success)
            {
                int dia = Convert.ToInt32(numerica.Groups[1].Value);
                int mes = Convert.ToInt32(numerica.Groups[2].Value);
                int anio = DateTime.Today.Year;

                if (numerica.Groups[3].Success)
                {
                    anio = Convert.ToInt32(numerica.Groups[3].Value);
                    if (anio < 100)
                        anio += 2000;
                }

                try
                {
                    fecha = new DateTime(anio, mes, dia);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            Match escrita = Regex.Match(
                normalizado,
                @"\b(\d{1,2})\s+de\s+(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)(?:\s+de\s+(\d{4}))?\b");

            if (escrita.Success)
            {
                int dia = Convert.ToInt32(escrita.Groups[1].Value);
                int mes = ObtenerNumeroMes(escrita.Groups[2].Value);
                int anio = DateTime.Today.Year;

                if (escrita.Groups[3].Success)
                    anio = Convert.ToInt32(escrita.Groups[3].Value);

                try
                {
                    fecha = new DateTime(anio, mes, dia);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            int diaSemana = ObtenerDiaSemanaDelTexto(normalizado);

            if (diaSemana > 0)
            {
                bool proximoEstricto = normalizado.Contains("proximo ");
                fecha = ObtenerProximoDiaSemana(diaSemana, proximoEstricto);
                return true;
            }

            return false;
        }

        private bool IntentarResolverHoraTexto(
            string texto,
            out TimeSpan hora)
        {
            hora = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(texto))
                return false;


            string normalizado =
                Normalizar(texto);


            // ============================================
            // FORMATO 11:30 / 11.30
            // ============================================

            Match match = Regex.Match(
                normalizado,
                @"\b([01]?\d|2[0-3])(?::|\.)([0-5]\d)\b");

            if (match.Success)
            {
                int horas =
                    Convert.ToInt32(
                        match.Groups[1].Value);

                int minutos =
                    Convert.ToInt32(
                        match.Groups[2].Value);

                hora =
                    new TimeSpan(
                        horas,
                        minutos,
                        0);

                return true;
            }


            // ============================================
            // FORMATO 11 AM / 11 PM
            // 11:30 AM / 11:30 PM
            // ============================================

            match = Regex.Match(
                normalizado,
                @"\b(1[0-2]|0?[1-9])" +
                @"(?::([0-5]\d))?\s*" +
                @"(am|pm)\b",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                int horas =
                    Convert.ToInt32(
                        match.Groups[1].Value);

                int minutos = 0;

                if (match.Groups[2].Success)
                {
                    minutos =
                        Convert.ToInt32(
                            match.Groups[2].Value);
                }


                string periodo =
                    match.Groups[3]
                        .Value
                        .ToLowerInvariant();


                if (periodo == "am")
                {
                    if (horas == 12)
                        horas = 0;
                }
                else
                {
                    if (horas != 12)
                        horas += 12;
                }


                hora =
                    new TimeSpan(
                        horas,
                        minutos,
                        0);

                return true;
            }


            // ============================================
            // FORMATO 11 HS / 11 H / 11 HORAS
            // ============================================

            match = Regex.Match(
                normalizado,
                @"\b([01]?\d|2[0-3])\s*" +
                @"(?:hs|h|hora|horas)\b",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                hora =
                    new TimeSpan(
                        Convert.ToInt32(
                            match.Groups[1].Value),
                        0,
                        0);

                return true;
            }


            // ============================================
            // FORMATO NATURAL:
            // "a las 11"
            // "a las 9"
            // "tipo 10"
            //
            // Esto solamente se ejecuta cuando el flujo
            // ya está esperando que el paciente elija
            // un horario, por lo que no confundimos
            // cualquier número con una hora.
            // ============================================

            match = Regex.Match(
                normalizado,
                @"\b(?:a\s+las|tipo)\s+" +
                @"([01]?\d|2[0-3])\b",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                hora =
                    new TimeSpan(
                        Convert.ToInt32(
                            match.Groups[1].Value),
                        0,
                        0);

                return true;
            }


            // ============================================
            // SI ESTAMOS ESPERANDO HORARIO,
            // PERMITIR TAMBIÉN ESCRIBIR SOLO:
            //
            // 11
            // 9
            // 15
            //
            // ============================================

            if (EstadoActual.Paso == "ESPERANDO_HORARIO")
            {
                match = Regex.Match(
                    normalizado,
                    @"^\s*([01]?\d|2[0-3])\s*$");

                if (match.Success)
                {
                    hora =
                        new TimeSpan(
                            Convert.ToInt32(
                                match.Groups[1].Value),
                            0,
                            0);

                    return true;
                }
            }


            return false;
        }

        private DateTime ObtenerLunesSemana(DateTime fecha)
        {
            int diferencia =
                (7 + ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday)) % 7;

            return fecha.Date.AddDays(-diferencia);
        }

        private DateTime ObtenerProximoDiaSemana(
            int diaSemana,
            bool estricto)
        {
            DateTime fecha = estricto
                ? DateTime.Today.AddDays(1)
                : DateTime.Today;

            for (int i = 0; i < 8; i++)
            {
                if (ConvertirDiaSemana(fecha.DayOfWeek) == diaSemana)
                    return fecha.Date;

                fecha = fecha.AddDays(1);
            }

            return fecha.Date;
        }

        private int ObtenerDiaSemanaDelTexto(string texto)
        {
            if (texto.Contains("lunes")) return 1;
            if (texto.Contains("martes")) return 2;
            if (texto.Contains("miercoles")) return 3;
            if (texto.Contains("jueves")) return 4;
            if (texto.Contains("viernes")) return 5;
            if (texto.Contains("sabado")) return 6;
            if (texto.Contains("domingo")) return 7;
            return 0;
        }

        private int ConvertirDiaSemana(DayOfWeek dia)
        {
            switch (dia)
            {
                case DayOfWeek.Monday: return 1;
                case DayOfWeek.Tuesday: return 2;
                case DayOfWeek.Wednesday: return 3;
                case DayOfWeek.Thursday: return 4;
                case DayOfWeek.Friday: return 5;
                case DayOfWeek.Saturday: return 6;
                case DayOfWeek.Sunday: return 7;
                default: return 0;
            }
        }

        private int ObtenerNumeroMes(string mes)
        {
            switch (mes)
            {
                case "enero": return 1;
                case "febrero": return 2;
                case "marzo": return 3;
                case "abril": return 4;
                case "mayo": return 5;
                case "junio": return 6;
                case "julio": return 7;
                case "agosto": return 8;
                case "septiembre": return 9;
                case "octubre": return 10;
                case "noviembre": return 11;
                case "diciembre": return 12;
                default: return 0;
            }
        }

        private bool EsConsultaClinicaBloqueada(string texto)
        {
            return texto.Contains("que medicamento") ||
                   texto.Contains("que remedio") ||
                   texto.Contains("que pastilla") ||
                   texto.Contains("que dosis") ||
                   texto.Contains("diagnostico") ||
                   texto.Contains("diagnosticar") ||
                   texto.Contains("que tratamiento") ||
                   texto.Contains("como tratar") ||
                   texto.Contains("recetar") ||
                   texto.Contains("prescribir");
        }

        private bool EsConsultaSobreOtroPaciente(string texto)
        {
            return texto.Contains("turnos de otro paciente") ||
                   texto.Contains("turnos de otra persona") ||
                   texto.Contains("quien tiene turno") ||
                   texto.Contains("que paciente") ||
                   texto.Contains("otro paciente");
        }

        private bool EsPedidoReinicio(string texto)
        {
            return texto == "reiniciar" ||
                   texto.Contains("empezar de nuevo") ||
                   texto.Contains("comenzar de nuevo") ||
                   texto.Contains("cancelar operacion");
        }

        private bool EsConfirmacion(string texto)
        {
            return texto == "si" ||
                   texto == "sí" ||
                   texto == "confirmar" ||
                   texto == "confirmo" ||
                   texto.Contains("si confirmar");
        }

        private bool EsNegacionOCambio(string texto)
        {
            return texto == "no" ||
                   texto.Contains("otro horario") ||
                   texto.Contains("cambiar horario") ||
                   texto.Contains("elegir otro");
        }

        private string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            string descompuesto = texto
                .Trim()
                .ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            StringBuilder limpio = new StringBuilder();

            foreach (char c in descompuesto)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo.GetUnicodeCategory(c);

                if (categoria != UnicodeCategory.NonSpacingMark)
                    limpio.Append(c);
            }

            return limpio
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        private string FormatearFecha(DateTime fecha)
        {
            return fecha.ToString(
                "dddd d 'de' MMMM 'de' yyyy",
                new CultureInfo("es-AR"));
        }

        private string FormatearFechaCorta(DateTime fecha)
        {
            return fecha.ToString(
                "ddd d MMM",
                new CultureInfo("es-AR"));
        }

        private void AgregarMensajePaciente(string texto)
        {
            Mensajes.Add(new MensajeChatTurnoIA(
                "Paciente",
                texto));
        }

        private void AgregarMensajeIA(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return;

            Mensajes.Add(new MensajeChatTurnoIA(
                "IA",
                texto));
        }

        private void DefinirOpciones(List<OpcionChatTurnoIA> opciones)
        {
            Opciones = opciones ?? new List<OpcionChatTurnoIA>();
        }

        private void ActualizarPantalla()
        {
            rptMensajes.DataSource = Mensajes;
            rptMensajes.DataBind();

            rptOpciones.DataSource = Opciones;
            rptOpciones.DataBind();

            pnlOpciones.Visible = Opciones.Count > 0;
        }

        protected void txtMensaje_TextChanged(object sender, EventArgs e)
        {
            btnEnviar_Click(sender, e);
        }
    }
}
