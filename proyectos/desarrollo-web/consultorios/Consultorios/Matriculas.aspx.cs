using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class Matriculas : System.Web.UI.Page
    {
        /* Profesional cuyas matriculas se estan administrando.
           Viene siempre por la URL. */
        private int IdProfesional
        {
            get
            {
                int id;

                if (int.TryParse(
                        Request.QueryString["idProfesional"], out id))
                {
                    return id;
                }

                return 0;
            }
        }


        /* Matricula que se esta editando. 0 = se esta dando de alta
           una nueva. Se guarda en ViewState para que sobreviva a los
           postbacks. */
        private int IdMatriculaEnEdicion
        {
            get
            {
                if (ViewState["IdMatriculaEnEdicion"] == null)
                    return 0;

                return (int)ViewState["IdMatriculaEnEdicion"];
            }
            set { ViewState["IdMatriculaEnEdicion"] = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo un admin con acceso a esta pantalla puede administrar matriculas.
            if (!RequerirAdministrador())
                return;

            if (!IsPostBack)
            {
                if (IdProfesional <= 0)
                {
                    lblMensaje.Text = "No se indico el profesional.";
                    pnlContenido.Visible = false;
                    return;
                }

                if (!CargarProfesional())
                    return;

                CargarTipos();
                CargarEstados();
                CargarGrilla();
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        private bool RequerirAdministrador()
        {
            return Seguridad.RequerirRol(this, "ADMIN") && Seguridad.RequerirAcceso(this);
        }


        private bool CargarProfesional()
        {
            try
            {
                Profesional profesional =
                    new ProfesionalNegocio().BuscarPorId(IdProfesional);

                if (profesional == null)
                {
                    lblMensaje.Text = "No se encontro el profesional.";
                    pnlContenido.Visible = false;
                    return false;
                }

                lblProfesional.Text = Server.HtmlEncode(
                    profesional.Apellido + ", " + profesional.Nombre +
                    " (" + profesional.Especialidad.Nombre + ")");

                return true;
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar el profesional: " + Server.HtmlEncode(ex.Message);

                pnlContenido.Visible = false;
                return false;
            }
        }


        private void CargarTipos()
        {
            ddlTipo.DataSource = new TipoMatriculaNegocio().Listar();
            ddlTipo.DataTextField = "Descripcion";
            ddlTipo.DataValueField = "IdTipoMatricula";
            ddlTipo.DataBind();

            ddlTipo.Items.Insert(
                0,
                new ListItem("-- Seleccione --", "")
            );
        }


        private void CargarEstados()
        {
            ddlEstado.DataSource = new EstadoNegocio().Listar();
            ddlEstado.DataTextField = "Descripcion";
            ddlEstado.DataValueField = "IdEstado";
            ddlEstado.DataBind();
        }


        private void CargarGrilla()
        {
            try
            {
                gvMatriculas.DataSource =
                    new MatriculaNegocio()
                        .ListarPorProfesional(IdProfesional);

                gvMatriculas.DataBind();

                gvHistorial.DataSource = new MatriculaNegocio().ListarHistorialPorProfesional(
                    IdProfesional, Convert.ToInt32(Session["IdUsuario"]));
                gvHistorial.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar el listado: " + Server.HtmlEncode(ex.Message);
            }
        }


        private void LimpiarFormulario()
        {
            IdMatriculaEnEdicion = 0;

            txtNumero.Text = "";
            txtEntidadEmisora.Text = "";
            txtVencimiento.Text = "";
            txtMotivo.Text = "";

            ddlTipo.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            litTituloForm.Text = "Nueva matricula";
            btnGuardar.Text = "Guardar matricula";
            btnCancelarEdicion.Visible = false;
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!RequerirAdministrador()) return;
            if (string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                lblMensaje.Text = "El numero de matricula es obligatorio.";
                return;
            }

            if (string.IsNullOrEmpty(ddlTipo.SelectedValue))
            {
                lblMensaje.Text = "Hay que elegir el tipo de matricula.";
                return;
            }

            try
            {
                Matricula matricula = new Matricula();

                matricula.IdProfesional = IdProfesional;
                matricula.Numero = txtNumero.Text.Trim();

                matricula.EntidadEmisora =
                    string.IsNullOrWhiteSpace(txtEntidadEmisora.Text)
                        ? null
                        : txtEntidadEmisora.Text.Trim();

                DateTime vencimiento;

                if (!string.IsNullOrWhiteSpace(txtVencimiento.Text))
                {
                    if (!DateTime.TryParseExact(txtVencimiento.Text, "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out vencimiento))
                        throw new ArgumentException("El vencimiento no tiene una fecha valida.");
                    matricula.FechaVencimiento = vencimiento;
                }

                matricula.IdTipoMatricula =
                    int.Parse(ddlTipo.SelectedValue);

                matricula.IdEstado =
                    int.Parse(ddlEstado.SelectedValue);

                MatriculaNegocio negocio = new MatriculaNegocio();
                int idAdmin = Convert.ToInt32(Session["IdUsuario"]);

                if (IdMatriculaEnEdicion > 0)
                {
                    matricula.IdMatricula = IdMatriculaEnEdicion;

                    negocio.Modificar(matricula, idAdmin, txtMotivo.Text);

                    lblMensaje.Text = "Matricula modificada.";
                }
                else
                {
                    negocio.Agregar(matricula, idAdmin, txtMotivo.Text);

                    lblMensaje.Text = "Matricula agregada.";
                }

                LimpiarFormulario();
                CargarGrilla();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "No se pudo guardar: " + Server.HtmlEncode(ex.Message);
            }
        }


        protected void btnCancelarEdicion_Click(object sender, EventArgs e)
        {
            if (!RequerirAdministrador()) return;
            lblMensaje.Text = "";
            LimpiarFormulario();
        }


        protected void gvMatriculas_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!RequerirAdministrador()) return;
            int idMatricula;

            if (!int.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idMatricula))
            {
                return;
            }

            try
            {
                MatriculaNegocio negocio = new MatriculaNegocio();

                if (e.CommandName == "Editar")
                {
                    Matricula matricula = negocio.BuscarPorId(idMatricula);

                    if (matricula == null)
                    {
                        lblMensaje.Text = "No se encontro la matricula.";
                        return;
                    }

                    if (matricula.IdProfesional != IdProfesional)
                        throw new InvalidOperationException("La matricula no corresponde a este profesional.");

                    IdMatriculaEnEdicion = matricula.IdMatricula;

                    txtNumero.Text = matricula.Numero;
                    txtEntidadEmisora.Text = matricula.EntidadEmisora;
                    txtMotivo.Text = "";

                    txtVencimiento.Text =
                        matricula.FechaVencimiento == null
                            ? ""
                            : matricula.FechaVencimiento
                                .Value.ToString("yyyy-MM-dd");

                    ddlTipo.SelectedValue =
                        matricula.IdTipoMatricula.ToString();

                    ddlEstado.SelectedValue =
                        matricula.IdEstado.ToString();

                    litTituloForm.Text = "Modificar matricula";
                    btnGuardar.Text = "Guardar cambios";
                    btnCancelarEdicion.Visible = true;

                    lblMensaje.Text = "";
                }
                else if (e.CommandName == "Baja")
                {
                    negocio.DarDeBaja(idMatricula, IdProfesional,
                        Convert.ToInt32(Session["IdUsuario"]), txtMotivo.Text);

                    lblMensaje.Text = "Matricula dada de baja.";

                    LimpiarFormulario();
                    CargarGrilla();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo completar la operacion: " + Server.HtmlEncode(ex.Message);
            }
        }
    }
}
