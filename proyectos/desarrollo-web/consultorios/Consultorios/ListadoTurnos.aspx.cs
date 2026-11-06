using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class ListadoTurnos :
        System.Web.UI.Page
    {
        // =====================================================
        // PERMISOS DEL USUARIO ACTUAL
        // =====================================================

        private bool puedeCrearTurnos;

        private bool puedeModificarTurnos;

        private bool puedeCancelarTurnos;


        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // Primero comprobamos acceso a la URL.

            if (!Seguridad.RequerirAcceso(this))
            {
                return;
            }


            // Además debe tener permiso para VER turnos.

            if (!Seguridad.RequerirPermiso(
                this,
                "TURNOS_VER"))
            {
                return;
            }


            // Cargamos los permisos de acción.
            // Esto se hace en cada request porque los eventos
            // ocurren después de Page_Load.

            CargarPermisosUsuario();


            if (!IsPostBack)
            {
                MostrarUsuario();

                ConfigurarVolver();

                ConfigurarInterfaz();

                CargarProfesionales();


                // Por defecto:
                // turnos del día actual.

                txtFecha.Text =
                    DateTime.Today
                        .ToString("yyyy-MM-dd");


                CargarGrilla();
            }
        }


        // =====================================================
        // PERMISOS
        // =====================================================

        private void CargarPermisosUsuario()
        {
            puedeCrearTurnos =
                Seguridad.TienePermiso(
                    "TURNOS_CREAR");


            puedeModificarTurnos =
                Seguridad.TienePermiso(
                    "TURNOS_MODIFICAR");


            puedeCancelarTurnos =
                Seguridad.TienePermiso(
                    "TURNOS_CANCELAR");
        }


        // =====================================================
        // INTERFAZ SEGÚN PERMISOS
        // =====================================================

        private void ConfigurarInterfaz()
        {
            btnNuevo.Visible =
                puedeCrearTurnos;
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
        // VOLVER AL PANEL CORRECTO
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
        // PROFESIONALES
        // =====================================================

        private void CargarProfesionales()
        {
            ddlProfesional.DataSource =
                new ProfesionalNegocio()
                    .ListarActivos();


            ddlProfesional.DataTextField =
                "Apellido";


            ddlProfesional.DataValueField =
                "IdProfesional";


            ddlProfesional.DataBind();


            ddlProfesional.Items.Insert(
                0,
                new ListItem(
                    "-- Todos --",
                    "")
            );
        }


        // =====================================================
        // GRILLA
        // =====================================================

        private void CargarGrilla()
        {
            try
            {
                TurnoNegocio negocio =
                    new TurnoNegocio();


                int idProfesional = 0;


                int.TryParse(
                    ddlProfesional.SelectedValue,
                    out idProfesional);


                DateTime fecha;


                bool hayFecha =
                    DateTime.TryParse(
                        txtFecha.Text,
                        out fecha);


                List<Turno> lista;


                if (idProfesional > 0 &&
                    hayFecha)
                {
                    lista =
                        negocio
                            .ListarPorProfesionalYFecha(
                                idProfesional,
                                fecha);
                }

                else if (idProfesional > 0)
                {
                    lista =
                        negocio
                            .ListarPorProfesional(
                                idProfesional);
                }

                else if (hayFecha)
                {
                    lista =
                        negocio
                            .ListarPorFecha(
                                fecha);
                }

                else
                {
                    lista =
                        negocio.Listar();
                }


                gvTurnos.DataSource =
                    lista;


                gvTurnos.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar el listado: "
                    +
                    ex.Message;
            }
        }


        // =====================================================
        // FILTRAR
        // =====================================================

        protected void BtnFiltrar_Click(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text = "";

            CargarGrilla();
        }


        // =====================================================
        // VER TODOS
        // =====================================================

        protected void BtnVerTodos_Click(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text = "";


            txtFecha.Text = "";


            ddlProfesional.SelectedIndex =
                0;


            CargarGrilla();
        }


        // =====================================================
        // NUEVO TURNO
        // =====================================================

        protected void BtnNuevo_Click(
            object sender,
            EventArgs e)
        {
            // Seguridad del lado servidor.
            // No alcanza con ocultar el botón.

            if (!Seguridad.TienePermiso(
                "TURNOS_CREAR"))
            {
                lblMensaje.Text =
                    "No tiene permiso para crear turnos.";

                return;
            }


            Response.Redirect(
                "AltaTurno.aspx");
        }


        // =====================================================
        // CONFIGURAR ACCIONES DE CADA FILA
        // =====================================================

        protected void GvTurnos_RowDataBound(
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


            LinkButton lnkConfirmar =
                e.Row.FindControl(
                    "lnkConfirmar")
                as LinkButton;


            LinkButton lnkAtendido =
                e.Row.FindControl(
                    "lnkAtendido")
                as LinkButton;


            LinkButton lnkAusente =
                e.Row.FindControl(
                    "lnkAusente")
                as LinkButton;


            LinkButton lnkCancelar =
                e.Row.FindControl(
                    "lnkCancelar")
                as LinkButton;


            // Editar / confirmar / atendido / ausente
            // dependen de TURNOS_MODIFICAR.

            if (lnkEditar != null)
            {
                lnkEditar.Visible =
                    puedeModificarTurnos;
            }


            if (lnkConfirmar != null)
            {
                lnkConfirmar.Visible =
                    puedeModificarTurnos;
            }


            if (lnkAtendido != null)
            {
                lnkAtendido.Visible =
                    puedeModificarTurnos;
            }


            if (lnkAusente != null)
            {
                lnkAusente.Visible =
                    puedeModificarTurnos;
            }


            // Cancelar tiene permiso independiente.

            if (lnkCancelar != null)
            {
                lnkCancelar.Visible =
                    puedeCancelarTurnos;
            }
        }


        // =====================================================
        // BUSCAR ESTADO
        // =====================================================

        private int BuscarIdEstado(
            string descripcion)
        {
            List<EstadoTurno> estados =
                new EstadoTurnoNegocio()
                    .Listar();


            foreach (EstadoTurno estado
                in estados)
            {
                if (estado.Descripcion.Equals(
                    descripcion,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return estado
                        .IdEstadoTurno;
                }
            }


            return 0;
        }


        // =====================================================
        // ACCIONES DE LA GRILLA
        // =====================================================

        protected void GvTurnos_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            int idTurno;


            if (!int.TryParse(
                Convert.ToString(
                    e.CommandArgument),
                out idTurno))
            {
                return;
            }


            try
            {
                TurnoNegocio negocio =
                    new TurnoNegocio();


                switch (e.CommandName)
                {
                    // =====================================
                    // EDITAR
                    // =====================================

                    case "Editar":

                        if (!Seguridad.TienePermiso(
                            "TURNOS_MODIFICAR"))
                        {
                            lblMensaje.Text =
                                "No tiene permiso para modificar turnos.";

                            return;
                        }


                        Response.Redirect(
                            "AltaTurno.aspx?id="
                            +
                            idTurno);

                        break;


                    // =====================================
                    // CONFIRMAR
                    // =====================================

                    case "Confirmar":

                        if (!Seguridad.TienePermiso(
                            "TURNOS_MODIFICAR"))
                        {
                            lblMensaje.Text =
                                "No tiene permiso para modificar turnos.";

                            return;
                        }


                        negocio.CambiarEstado(
                            idTurno,
                            BuscarIdEstado(
                                "Confirmado"));


                        lblMensaje.Text =
                            "Turno confirmado.";


                        CargarGrilla();

                        break;


                    // =====================================
                    // ATENDIDO
                    // =====================================

                    case "Atendido":

                        if (!Seguridad.TienePermiso(
                            "TURNOS_MODIFICAR"))
                        {
                            lblMensaje.Text =
                                "No tiene permiso para modificar turnos.";

                            return;
                        }


                        negocio.CambiarEstado(
                            idTurno,
                            BuscarIdEstado(
                                "Atendido"));


                        lblMensaje.Text =
                            "Turno marcado como atendido.";


                        CargarGrilla();

                        break;


                    // =====================================
                    // AUSENTE
                    // =====================================

                    case "Ausente":

                        if (!Seguridad.TienePermiso(
                            "TURNOS_MODIFICAR"))
                        {
                            lblMensaje.Text =
                                "No tiene permiso para modificar turnos.";

                            return;
                        }


                        negocio.CambiarEstado(
                            idTurno,
                            BuscarIdEstado(
                                "Ausente"));


                        lblMensaje.Text =
                            "Turno marcado como ausente.";


                        CargarGrilla();

                        break;


                    // =====================================
                    // CANCELAR
                    // =====================================

                    case "Cancelar":

                        if (!Seguridad.TienePermiso(
                            "TURNOS_CANCELAR"))
                        {
                            lblMensaje.Text =
                                "No tiene permiso para cancelar turnos.";

                            return;
                        }


                        negocio.Cancelar(
                            idTurno);


                        lblMensaje.Text =
                            "Turno cancelado.";


                        CargarGrilla();

                        break;
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
