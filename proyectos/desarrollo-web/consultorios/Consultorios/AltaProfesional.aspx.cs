using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class AltaProfesional : System.Web.UI.Page
    {
        /* Si la URL trae ?id=N estamos modificando ese profesional.
           Si no trae nada, es un alta. */
        private int IdProfesional
        {
            get
            {
                int id;

                if (int.TryParse(Request.QueryString["id"], out id))
                    return id;

                return 0;
            }
        }

        private bool EsModificacion
        {
            get { return IdProfesional > 0; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            /* El acceso se decide por nivel y URL (tabla Nivel_URL),
               no por un rol fijo. Ver Seguridad.RequerirAcceso. */
            if (!Seguridad.RequerirAcceso(this))
                return;

            if (!IsPostBack)
            {
                CargarEspecialidades();
                CargarEstados();
                CargarTiposMatricula();

                if (EsModificacion)
                {
                    litTitulo.Text = "Modificar profesional";

                    /* La matricula se carga desde su propia pantalla,
                       aca solo sirve para el alta. */
                    pnlMatriculaInicial.Visible = false;

                    CargarProfesional();
                }
            }
        }


        private void CargarEspecialidades()
        {
            EspecialidadNegocio negocio = new EspecialidadNegocio();

            ddlEspecialidad.DataSource = negocio.Listar();
            ddlEspecialidad.DataTextField = "Nombre";
            ddlEspecialidad.DataValueField = "IdEspecialidad";
            ddlEspecialidad.DataBind();

            ddlEspecialidad.Items.Insert(
                0,
                new ListItem("-- Seleccione --", "")
            );
        }


        private void CargarEstados()
        {
            EstadoNegocio negocio = new EstadoNegocio();

            ddlEstado.DataSource = negocio.Listar();
            ddlEstado.DataTextField = "Descripcion";
            ddlEstado.DataValueField = "IdEstado";
            ddlEstado.DataBind();
        }


        private void CargarTiposMatricula()
        {
            TipoMatriculaNegocio negocio = new TipoMatriculaNegocio();

            ddlTipoMatricula.DataSource = negocio.Listar();
            ddlTipoMatricula.DataTextField = "Descripcion";
            ddlTipoMatricula.DataValueField = "IdTipoMatricula";
            ddlTipoMatricula.DataBind();

            ddlTipoMatricula.Items.Insert(
                0,
                new ListItem("-- Seleccione --", "")
            );
        }


        private void CargarProfesional()
        {
            try
            {
                ProfesionalNegocio negocio = new ProfesionalNegocio();

                Profesional profesional =
                    negocio.BuscarPorId(IdProfesional);

                if (profesional == null)
                {
                    lblMensaje.Text = "No se encontró el profesional.";
                    btnGuardar.Enabled = false;
                    return;
                }

                txtNombre.Text = profesional.Nombre;
                txtApellido.Text = profesional.Apellido;
                txtFoto.Text = profesional.Foto;

                ddlEspecialidad.SelectedValue =
                    profesional.IdEspecialidad.ToString();

                ddlEstado.SelectedValue =
                    profesional.IdEstado.ToString();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar el profesional: " + ex.Message;
            }
        }


        /* Devuelve "" si esta todo bien, o el texto del error. */
        private string Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                return "El nombre es obligatorio.";

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
                return "El apellido es obligatorio.";

            if (string.IsNullOrEmpty(ddlEspecialidad.SelectedValue))
                return "Hay que elegir una especialidad.";

            /* Si se empezo a cargar la matricula, tiene que estar
               completa: numero y tipo. */
            if (!EsModificacion
                && !string.IsNullOrWhiteSpace(txtNumeroMatricula.Text)
                && string.IsNullOrEmpty(ddlTipoMatricula.SelectedValue))
            {
                return "Si cargás una matrícula, elegí el tipo.";
            }

            return "";
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            string error = Validar();

            if (error != "")
            {
                lblMensaje.Text = error;
                return;
            }

            try
            {
                Profesional profesional = new Profesional();

                profesional.Nombre = txtNombre.Text.Trim();
                profesional.Apellido = txtApellido.Text.Trim();

                profesional.Foto =
                    string.IsNullOrWhiteSpace(txtFoto.Text)
                        ? null
                        : txtFoto.Text.Trim();

                profesional.IdEspecialidad =
                    int.Parse(ddlEspecialidad.SelectedValue);

                profesional.IdEstado =
                    int.Parse(ddlEstado.SelectedValue);

                if (EsModificacion)
                {
                    profesional.IdProfesional = IdProfesional;

                    new ProfesionalNegocio().Modificar(profesional);
                }
                else if (!string.IsNullOrWhiteSpace(txtNumeroMatricula.Text))
                {
                    /* Alta del profesional junto con su primera
                       matricula, en una sola transaccion. */
                    Matricula matricula = new Matricula();

                    matricula.Numero = txtNumeroMatricula.Text.Trim();

                    matricula.EntidadEmisora =
                        string.IsNullOrWhiteSpace(txtEntidadEmisora.Text)
                            ? null
                            : txtEntidadEmisora.Text.Trim();

                    matricula.IdTipoMatricula =
                        int.Parse(ddlTipoMatricula.SelectedValue);

                    DateTime vencimiento;

                    if (DateTime.TryParse(txtVencimiento.Text, out vencimiento))
                        matricula.FechaVencimiento = vencimiento;

                    new MatriculaNegocio()
                        .AgregarProfesionalConMatricula(profesional, matricula,
                            Convert.ToInt32(Session["IdUsuario"]));
                }
                else
                {
                    new ProfesionalNegocio().Agregar(profesional);
                }

                Response.Redirect("ListadoProfesionales.aspx");
            }
            catch (System.Threading.ThreadAbortException)
            {
                /* La lanza Response.Redirect cuando sale todo bien.
                   No es un error, hay que dejarla pasar. */
                throw;
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "No se pudo guardar: " + ex.Message;
            }
        }


        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("ListadoProfesionales.aspx");
        }
    }
}
