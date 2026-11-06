using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class AltaTurno :
        System.Web.UI.Page
    {
        // =====================================================
        // ID DEL TURNO
        // =====================================================

        private int IdTurno
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["id"],
                    out id))
                {
                    return id;
                }

                return 0;
            }
        }


        // =====================================================
        // MODO DE LA PANTALLA
        // =====================================================

        private bool EsModificacion
        {
            get
            {
                return IdTurno > 0;
            }
        }


        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // Primero debe poder entrar a AltaTurno.aspx
            // según Nivel_URL.

            if (!Seguridad.RequerirAcceso(this))
            {
                return;
            }


            // =============================================
            // PERMISO SEGÚN MODO
            // =============================================

            if (EsModificacion)
            {
                // /AltaTurno.aspx?id=123

                if (!Seguridad.RequerirPermiso(
                    this,
                    "TURNOS_MODIFICAR"))
                {
                    return;
                }
            }
            else
            {
                // /AltaTurno.aspx

                if (!Seguridad.RequerirPermiso(
                    this,
                    "TURNOS_CREAR"))
                {
                    return;
                }
            }


            if (!IsPostBack)
            {
                CargarPacientes();

                CargarProfesionales();

                CargarEstadosTurno();


                if (EsModificacion)
                {
                    litTitulo.Text =
                        "Modificar turno";

                    btnGuardar.Text =
                        "Guardar cambios";

                    CargarTurno();
                }
                else
                {
                    litTitulo.Text =
                        "Nuevo turno";

                    btnGuardar.Text =
                        "Crear turno";

                    txtFecha.Text =
                        DateTime.Today
                            .ToString("yyyy-MM-dd");
                }
            }
        }


        // =====================================================
        // PACIENTES
        // =====================================================

        private void CargarPacientes()
        {
            List<Paciente> pacientes =
                new PacienteNegocio()
                    .Listar();


            ddlPaciente.Items.Clear();


            ddlPaciente.Items.Add(
                new ListItem(
                    "-- Seleccione --",
                    "")
            );


            foreach (Paciente paciente
                in pacientes)
            {
                ddlPaciente.Items.Add(
                    new ListItem(
                        paciente.Apellido
                        +
                        ", "
                        +
                        paciente.Nombre,

                        paciente.IdPaciente
                            .ToString()
                    )
                );
            }
        }


        // =====================================================
        // PROFESIONALES
        // =====================================================

        private void CargarProfesionales()
        {
            List<Profesional> profesionales =
                new ProfesionalNegocio()
                    .ListarActivos();


            ddlProfesional.Items.Clear();


            ddlProfesional.Items.Add(
                new ListItem(
                    "-- Seleccione --",
                    "")
            );


            foreach (Profesional profesional
                in profesionales)
            {
                ddlProfesional.Items.Add(
                    new ListItem(
                        profesional.Apellido
                        +
                        ", "
                        +
                        profesional.Nombre
                        +
                        " ("
                        +
                        profesional
                            .Especialidad
                            .Nombre
                        +
                        ")",

                        profesional
                            .IdProfesional
                            .ToString()
                    )
                );
            }
        }


        // =====================================================
        // ESTADOS
        // =====================================================

        private void CargarEstadosTurno()
        {
            ddlEstadoTurno.DataSource =
                new EstadoTurnoNegocio()
                    .Listar();


            ddlEstadoTurno.DataTextField =
                "Descripcion";


            ddlEstadoTurno.DataValueField =
                "IdEstadoTurno";


            ddlEstadoTurno.DataBind();
        }


        // =====================================================
        // CARGAR TURNO EXISTENTE
        // =====================================================

        private void CargarTurno()
        {
            try
            {
                Turno turno =
                    new TurnoNegocio()
                        .BuscarPorId(
                            IdTurno);


                if (turno == null)
                {
                    lblMensaje.Text =
                        "No se encontró el turno.";

                    btnGuardar.Enabled =
                        false;

                    return;
                }


                ddlPaciente.SelectedValue =
                    turno.IdPaciente
                        .ToString();


                ddlProfesional.SelectedValue =
                    turno.IdProfesional
                        .ToString();


                ddlEstadoTurno.SelectedValue =
                    turno.IdEstadoTurno
                        .ToString();


                txtFecha.Text =
                    turno.FechaHora
                        .ToString(
                            "yyyy-MM-dd");


                txtHora.Text =
                    turno.FechaHora
                        .ToString(
                            "HH:mm");


                txtDuracion.Text =
                    turno.DuracionMinutos
                        .ToString();


                txtMotivo.Text =
                    turno.Motivo;


                txtObservaciones.Text =
                    turno.Observaciones;


                MostrarAgenda();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar el turno: "
                    +
                    ex.Message;

                btnGuardar.Enabled =
                    false;
            }
        }


        // =====================================================
        // MOSTRAR AGENDA DEL PROFESIONAL
        // =====================================================

        private void MostrarAgenda()
        {
            int idProfesional;


            if (!int.TryParse(
                ddlProfesional.SelectedValue,
                out idProfesional))
            {
                pnlAgenda.Visible =
                    false;

                return;
            }


            try
            {
                List<Agenda> agendas =
                    new AgendaNegocio()
                        .ListarPorProfesional(
                            idProfesional);


                if (agendas.Count == 0)
                {
                    litAgenda.Text =
                        "este profesional no tiene horarios cargados.";

                    pnlAgenda.Visible =
                        true;

                    return;
                }


                StringBuilder texto =
                    new StringBuilder();


                foreach (Agenda agenda
                    in agendas)
                {
                    if (agenda.Estado.Descripcion !=
                        "Activo")
                    {
                        continue;
                    }


                    if (texto.Length > 0)
                    {
                        texto.Append(
                            " | ");
                    }


                    texto.Append(
                        agenda.NombreDia);

                    texto.Append(
                        " ");

                    texto.Append(
                        agenda.Horario);
                }


                litAgenda.Text =
                    texto.Length == 0
                        ? "no tiene horarios activos."
                        : texto.ToString();


                pnlAgenda.Visible =
                    true;
            }
            catch (Exception)
            {
                pnlAgenda.Visible =
                    false;
            }
        }


        // =====================================================
        // CAMBIO DE PROFESIONAL
        // =====================================================

        protected void DdlProfesional_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            MostrarAgenda();
        }


        // =====================================================
        // VALIDACIONES
        // =====================================================

        private string Validar(
            out DateTime fechaHora)
        {
            fechaHora =
                DateTime.MinValue;


            if (string.IsNullOrEmpty(
                ddlPaciente.SelectedValue))
            {
                return
                    "Hay que elegir un paciente.";
            }


            if (string.IsNullOrEmpty(
                ddlProfesional.SelectedValue))
            {
                return
                    "Hay que elegir un profesional.";
            }


            DateTime fecha;


            if (!DateTime.TryParse(
                txtFecha.Text,
                out fecha))
            {
                return
                    "La fecha no es válida.";
            }


            TimeSpan hora;


            if (!TimeSpan.TryParse(
                txtHora.Text,
                out hora))
            {
                return
                    "La hora no es válida.";
            }


            fechaHora =
                fecha.Date.Add(
                    hora);


            int duracion;


            if (!int.TryParse(
                txtDuracion.Text,
                out duracion)
                ||
                duracion <= 0)
            {
                return
                    "La duración tiene que ser un número mayor que cero.";
            }


            int idProfesional =
                int.Parse(
                    ddlProfesional.SelectedValue);


            // =============================================
            // REGLA 1:
            // HORARIO DE ATENCIÓN
            // =============================================

            AgendaNegocio agendaNegocio =
                new AgendaNegocio();


            if (!agendaNegocio
                    .SinAgendaCargada(
                        idProfesional)
                &&
                !agendaNegocio
                    .CubreHorario(
                        idProfesional,
                        fechaHora,
                        duracion))
            {
                return
                    "El profesional no atiende ese día a esa hora. "
                    +
                    "Revisá los horarios que figuran más arriba.";
            }


            // =============================================
            // REGLA 2:
            // EVITAR SOLAPAMIENTOS
            // =============================================

            TurnoNegocio turnoNegocio =
                new TurnoNegocio();


            if (turnoNegocio
                .HaySolapamiento(
                    idProfesional,
                    fechaHora,
                    duracion,
                    EsModificacion
                        ? IdTurno
                        : 0))
            {
                return
                    "El profesional ya tiene otro turno que se "
                    +
                    "superpone con ese horario.";
            }


            return "";
        }


        // =====================================================
        // GUARDAR
        // =====================================================

        protected void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            // =============================================
            // SEGURIDAD DEL LADO SERVIDOR
            // =============================================

            if (EsModificacion)
            {
                if (!Seguridad.TienePermiso(
                    "TURNOS_MODIFICAR"))
                {
                    lblMensaje.Text =
                        "No tiene permiso para modificar turnos.";

                    return;
                }
            }
            else
            {
                if (!Seguridad.TienePermiso(
                    "TURNOS_CREAR"))
                {
                    lblMensaje.Text =
                        "No tiene permiso para crear turnos.";

                    return;
                }
            }


            // =============================================
            // VALIDACIONES
            // =============================================

            DateTime fechaHora;


            string error =
                Validar(
                    out fechaHora);


            if (error != "")
            {
                lblMensaje.Text =
                    error;

                return;
            }


            try
            {
                Turno turno =
                    new Turno();


                turno.FechaHora =
                    fechaHora;


                turno.DuracionMinutos =
                    int.Parse(
                        txtDuracion.Text);


                turno.Motivo =
                    string.IsNullOrWhiteSpace(
                        txtMotivo.Text)
                        ? null
                        : txtMotivo.Text.Trim();


                turno.Observaciones =
                    string.IsNullOrWhiteSpace(
                        txtObservaciones.Text)
                        ? null
                        : txtObservaciones.Text.Trim();


                turno.IdPaciente =
                    int.Parse(
                        ddlPaciente.SelectedValue);


                turno.IdProfesional =
                    int.Parse(
                        ddlProfesional.SelectedValue);


                turno.IdEstadoTurno =
                    int.Parse(
                        ddlEstadoTurno.SelectedValue);


                TurnoNegocio negocio =
                    new TurnoNegocio();


                // =========================================
                // MODIFICAR
                // =========================================

                if (EsModificacion)
                {
                    // Segunda comprobación justo antes
                    // de modificar datos.

                    if (!Seguridad.TienePermiso(
                        "TURNOS_MODIFICAR"))
                    {
                        lblMensaje.Text =
                            "No tiene permiso para modificar turnos.";

                        return;
                    }


                    turno.IdTurno =
                        IdTurno;


                    negocio.Modificar(
                        turno);
                }

                // =========================================
                // CREAR
                // =========================================

                else
                {
                    // Segunda comprobación justo antes
                    // de insertar datos.

                    if (!Seguridad.TienePermiso(
                        "TURNOS_CREAR"))
                    {
                        lblMensaje.Text =
                            "No tiene permiso para crear turnos.";

                        return;
                    }


                    negocio.Agregar(
                        turno);
                }


                Response.Redirect(
                    "ListadoTurnos.aspx");
            }
            catch (
                System.Threading
                    .ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo guardar: "
                    +
                    ex.Message;
            }
        }


        // =====================================================
        // CANCELAR FORMULARIO
        // =====================================================

        protected void BtnCancelar_Click(
            object sender,
            EventArgs e)
        {
            Response.Redirect(
                "ListadoTurnos.aspx");
        }
    }
}
