using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class ListadoProfesionales : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /* El acceso se decide por nivel y URL (tabla Nivel_URL),
               no por un rol fijo. Ver Seguridad.RequerirAcceso. */
            if (!Seguridad.RequerirAcceso(this))
                return;

            if (!IsPostBack)
            {
                lblUsuario.Text =
                    Session["Nivel"] + ": " +
                    Session["Nombre"] + " " + Session["Apellido"];

                CargarGrilla();
            }
        }


        /* Si el cuadro de busqueda esta vacio trae todos.
           ProfesionalNegocio.Buscar ya contempla ese caso. */
        private void CargarGrilla()
        {
            try
            {
                ProfesionalNegocio negocio = new ProfesionalNegocio();

                gvProfesionales.DataSource =
                    negocio.Buscar(txtBuscar.Text);

                gvProfesionales.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo cargar el listado: " + ex.Message;
            }
        }


        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = "";
            CargarGrilla();
        }


        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            lblMensaje.Text = "";
            CargarGrilla();
        }


        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Response.Redirect("AltaProfesional.aspx");
        }


        protected void gvProfesionales_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            /* Los comandos propios del GridView (paginar, ordenar)
               no traen un id valido, se ignoran. */
            int idProfesional;

            if (!int.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idProfesional))
            {
                return;
            }

            try
            {
                ProfesionalNegocio negocio = new ProfesionalNegocio();

                switch (e.CommandName)
                {
                    case "Editar":
                        Response.Redirect(
                            "AltaProfesional.aspx?id=" + idProfesional);
                        break;

                    case "Matriculas":
                        Response.Redirect(
                            "Matriculas.aspx?idProfesional=" + idProfesional);
                        break;

                    case "Baja":
                        negocio.DarDeBaja(idProfesional);
                        lblMensaje.Text = "Profesional dado de baja.";
                        CargarGrilla();
                        break;

                    case "Reactivar":
                        negocio.Reactivar(idProfesional);
                        lblMensaje.Text = "Profesional reactivado.";
                        CargarGrilla();
                        break;
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo completar la operación: " + ex.Message;
            }
        }
    }
}
