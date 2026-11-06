using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class AgendaProfesional :
        System.Web.UI.Page
    {
        // =====================================================
        // PERMISOS
        // =====================================================

        private bool puedeModificarAgenda;


        // =====================================================
        // AGENDA EN EDICIÓN
        // =====================================================

        private int IdAgendaEnEdicion
        {
            get
            {
                if (ViewState["IdAgendaEnEdicion"] == null)
                {
                    return 0;
                }

                return
                    (int)ViewState[
                        "IdAgendaEnEdicion"];
            }

            set
            {
                ViewState[
                    "IdAgendaEnEdicion"] =
                    value;
            }
        }


        // =====================================================
        // PROFESIONAL SELECCIONADO
        // =====================================================

        private int IdProfesionalSeleccionado
        {
            get
            {
                int id;


                if (int.TryParse(
                    ddlProfesional.SelectedValue,
                    out id))
                {
                    return id;
                }


                return 0;
            }
        }


        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // Primero debe tener acceso a la URL.

            if (!Seguridad.RequerirAcceso(this))
            {
                return;
            }


            // Además debe tener permiso para VER agenda.

            if (!Seguridad.RequerirPermiso(
                this,
                "AGENDA_VER"))
            {
                return;
            }


            // Se consulta en cada request porque
            // RowCommand ocurre después de Page_Load.

            puedeModificarAgenda =
                Seguridad.TienePermiso(
                    "AGENDA_MODIFICAR");


            if (!IsPostBack)
            {
                MostrarUsuario();

                ConfigurarVolver();

                ConfigurarInterfaz();

                CargarProfesionales();


                if (puedeModificarAgenda)
                {
                    CargarEstados();
                }
            }
        }


        // =====================================================
        // USUARIO
        // =====================================================

        private void MostrarUsuario()
        {
            lblUsuario.Text =
                Convert.ToString(
                    Session["Nivel"])
                +
                ": "
                +
                Convert.ToString(
                    Session["Nombre"])
                +
                " "
                +
                Convert.ToString(
                    Session["Apellido"]);
        }


        // =====================================================
        // VOLVER AL PANEL
        // =====================================================

        private void ConfigurarVolver()
        {
            string nivel =
                Convert.ToString(
                    Session["Nivel"]);


            switch (nivel)
            {
                case "ADMIN":

                    lnkVolver.NavigateUrl =
                        "~/PanelAdmin.aspx";

                    break;


                case "RECEPCIONISTA":

                    lnkVolver.NavigateUrl =
                        "~/PanelRecepcionista.aspx";

                    break;


                case "MEDICO":

                    lnkVolver.NavigateUrl =
                        "~/PanelMedico.aspx";

                    break;


                case "PACIENTE":

                    lnkVolver.NavigateUrl =
                        "~/PanelPaciente.aspx";

                    break;


                default:

                    lnkVolver.NavigateUrl =
                        "~/Login.aspx";

                    break;
            }
        }


        // =====================================================
        // INTERFAZ SEGÚN PERMISOS
        // =====================================================

        private void ConfigurarInterfaz()
        {
            pnlFormulario.Visible =
                puedeModificarAgenda;


            pnlSoloLectura.Visible =
                !puedeModificarAgenda;
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

        private void CargarEstados()
        {
            ddlEstado.DataSource =
                new EstadoNegocio()
                    .Listar();


            ddlEstado.DataTextField =
                "Descripcion";


            ddlEstado.DataValueField =
                "IdEstado";


            ddlEstado.DataBind();
        }


        // =====================================================
        // GRILLA
        // =====================================================

        private void CargarGrilla()
        {
            try
            {
                gvAgenda.DataSource =
                    new AgendaNegocio()
                        .ListarPorProfesional(
                            IdProfesionalSeleccionado);


                gvAgenda.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar la agenda: "
                    +
                    ex.Message;
            }
        }


        // =====================================================
        // LIMPIAR FORMULARIO
        // =====================================================

        private void LimpiarFormulario()
        {
            IdAgendaEnEdicion =
                0;


            ddlDiaSemana.SelectedIndex =
                0;


            ddlAtencion.SelectedIndex =
                0;


            if (ddlEstado.Items.Count > 0)
            {
                ddlEstado.SelectedIndex =
                    0;
            }


            txtHoraInicio.Text =
                "";


            txtHoraFin.Text =
                "";


            txtDuracion.Text =
                "30";


            litTituloForm.Text =
                "Nueva franja horaria";


            btnGuardar.Text =
                "Guardar franja";


            btnCancelarEdicion.Visible =
                false;
        }


        // =====================================================
        // CAMBIO DE PROFESIONAL
        // =====================================================

        protected void DdlProfesional_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text =
                "";


            if (IdProfesionalSeleccionado == 0)
            {
                pnlContenido.Visible =
                    false;

                return;
            }


            pnlContenido.Visible =
                true;


            if (puedeModificarAgenda)
            {
                LimpiarFormulario();
            }


            CargarGrilla();
        }


        // =====================================================
        // VALIDAR
        // =====================================================

        private string Validar(
            out TimeSpan inicio,
            out TimeSpan fin)
        {
            inicio =
                TimeSpan.Zero;


            fin =
                TimeSpan.Zero;


            if (IdProfesionalSeleccionado == 0)
            {
                return
                    "Hay que elegir un profesional.";
            }


            if (!TimeSpan.TryParse(
                txtHoraInicio.Text,
                out inicio))
            {
                return
                    "La hora de inicio no es válida.";
            }


            if (!TimeSpan.TryParse(
                txtHoraFin.Text,
                out fin))
            {
                return
                    "La hora de fin no es válida.";
            }


            if (fin <= inicio)
            {
                return
                    "La hora de fin tiene que ser posterior a la de inicio.";
            }


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


            return "";
        }


        // =====================================================
        // GUARDAR
        // =====================================================

        protected void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            // No alcanza con ocultar el formulario.
            // Se controla nuevamente en servidor.

            if (!Seguridad.TienePermiso(
                "AGENDA_MODIFICAR"))
            {
                lblMensaje.Text =
                    "No tiene permiso para modificar la agenda.";

                return;
            }


            TimeSpan inicio;

            TimeSpan fin;


            string error =
                Validar(
                    out inicio,
                    out fin);


            if (error != "")
            {
                lblMensaje.Text =
                    error;

                return;
            }


            try
            {
                Agenda agenda =
                    new Agenda();


                agenda.IdProfesional =
                    IdProfesionalSeleccionado;


                agenda.DiaSemana =
                    int.Parse(
                        ddlDiaSemana.SelectedValue);


                agenda.HoraInicio =
                    inicio;


                agenda.HoraFin =
                    fin;


                agenda.DuracionMinutos =
                    int.Parse(
                        txtDuracion.Text);


                agenda.AtencionConTurno =
                    ddlAtencion.SelectedValue
                    ==
                    "1";


                agenda.IdEstado =
                    int.Parse(
                        ddlEstado.SelectedValue);


                AgendaNegocio negocio =
                    new AgendaNegocio();


                // =========================================
                // MODIFICAR
                // =========================================

                if (IdAgendaEnEdicion > 0)
                {
                    // Segunda validación antes de modificar.

                    if (!Seguridad.TienePermiso(
                        "AGENDA_MODIFICAR"))
                    {
                        lblMensaje.Text =
                            "No tiene permiso para modificar la agenda.";

                        return;
                    }


                    agenda.IdAgenda =
                        IdAgendaEnEdicion;


                    negocio.Modificar(
                        agenda);


                    lblMensaje.Text =
                        "Franja horaria modificada.";
                }

                // =========================================
                // CREAR
                // =========================================

                else
                {
                    // Segunda validación antes de insertar.

                    if (!Seguridad.TienePermiso(
                        "AGENDA_MODIFICAR"))
                    {
                        lblMensaje.Text =
                            "No tiene permiso para modificar la agenda.";

                        return;
                    }


                    negocio.Agregar(
                        agenda);


                    lblMensaje.Text =
                        "Franja horaria agregada.";
                }


                LimpiarFormulario();

                CargarGrilla();
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
        // CANCELAR EDICIÓN
        // =====================================================

        protected void BtnCancelarEdicion_Click(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text =
                "";


            if (!puedeModificarAgenda)
            {
                return;
            }


            LimpiarFormulario();
        }


        // =====================================================
        // CONFIGURAR FILAS SEGÚN PERMISOS
        // =====================================================

        protected void GvAgenda_RowDataBound(
            object sender,
            GridViewRowEventArgs e)
        {
            if (e.Row.RowType !=
                DataControlRowType.DataRow)
            {
                return;
            }


            LinkButton lnkEditar =
                e.Row.FindControl(
                    "lnkEditar")
                as LinkButton;


            LinkButton lnkBaja =
                e.Row.FindControl(
                    "lnkBaja")
                as LinkButton;


            Literal litSeparador =
                e.Row.FindControl(
                    "litSeparador")
                as Literal;


            if (lnkEditar != null)
            {
                lnkEditar.Visible =
                    puedeModificarAgenda;
            }


            if (lnkBaja != null)
            {
                lnkBaja.Visible =
                    puedeModificarAgenda;
            }


            if (litSeparador != null)
            {
                litSeparador.Visible =
                    puedeModificarAgenda;
            }
        }


        // =====================================================
        // ACCIONES DE LA GRILLA
        // =====================================================

        protected void GvAgenda_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            int idAgenda;


            if (!int.TryParse(
                Convert.ToString(
                    e.CommandArgument),
                out idAgenda))
            {
                return;
            }


            // Tanto Editar como Baja modifican datos.

            if (e.CommandName == "Editar" ||
                e.CommandName == "Baja")
            {
                if (!Seguridad.TienePermiso(
                    "AGENDA_MODIFICAR"))
                {
                    lblMensaje.Text =
                        "No tiene permiso para modificar la agenda.";

                    return;
                }
            }


            try
            {
                AgendaNegocio negocio =
                    new AgendaNegocio();


                // =========================================
                // EDITAR
                // =========================================

                if (e.CommandName == "Editar")
                {
                    Agenda agenda =
                        negocio.BuscarPorId(
                            idAgenda);


                    if (agenda == null)
                    {
                        lblMensaje.Text =
                            "No se encontró la franja.";

                        return;
                    }


                    IdAgendaEnEdicion =
                        agenda.IdAgenda;


                    ddlDiaSemana.SelectedValue =
                        agenda.DiaSemana
                            .ToString();


                    txtHoraInicio.Text =
                        agenda.HoraInicio
                            .ToString(
                                @"hh\:mm");


                    txtHoraFin.Text =
                        agenda.HoraFin
                            .ToString(
                                @"hh\:mm");


                    txtDuracion.Text =
                        agenda.DuracionMinutos
                            .ToString();


                    ddlAtencion.SelectedValue =
                        agenda.AtencionConTurno
                            ? "1"
                            : "0";


                    ddlEstado.SelectedValue =
                        agenda.IdEstado
                            .ToString();


                    litTituloForm.Text =
                        "Modificar franja horaria";


                    btnGuardar.Text =
                        "Guardar cambios";


                    btnCancelarEdicion.Visible =
                        true;


                    lblMensaje.Text =
                        "";
                }

                // =========================================
                // DAR DE BAJA
                // =========================================

                else if (e.CommandName == "Baja")
                {
                    // Validación nuevamente justo antes
                    // de modificar datos.

                    if (!Seguridad.TienePermiso(
                        "AGENDA_MODIFICAR"))
                    {
                        lblMensaje.Text =
                            "No tiene permiso para modificar la agenda.";

                        return;
                    }


                    negocio.DarDeBaja(
                        idAgenda);


                    lblMensaje.Text =
                        "Franja horaria dada de baja.";


                    LimpiarFormulario();

                    CargarGrilla();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo completar la operación: "
                    +
                    ex.Message;
            }
        }
    }
}
