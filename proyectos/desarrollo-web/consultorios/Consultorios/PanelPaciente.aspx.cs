using System;
using BIZ;

namespace Consultorios
{
    public partial class PanelPaciente : System.Web.UI.Page
    {
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    if (!Seguridad.RequerirAcceso(this))
        //        return;

        //    if (!IsPostBack)
        //    {
        //        lblUsuario.Text =
        //        "Paciente: " +
        //         Session["Nombre"] + " " + Session["Apellido"];
        //    }
        //}

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            if (!IsPostBack)
            {
                lnkMisTurnos.Visible = new UsuarioDAO().TieneAcceso(Convert.ToInt32(Session["IdNivel"]), "MisTurnosPaciente.aspx");
                lnkHistorial.Visible = lnkMisTurnos.Visible;
                lblUsuario.Text = Server.HtmlEncode(
                    "Cuenta: " +
                    Session["Nombre"] + " " + Session["Apellido"]
                );

                int idUsuario =
                    Convert.ToInt32(Session["IdUsuario"]);

                PacienteNegocio negocio = new PacienteNegocio();

                Paciente paciente =
                    negocio.BuscarPorUsuario(idUsuario);

                if (paciente == null)
                {
                    lblDatosPaciente.Text =
                        "Tu cuenta todavia no tiene un paciente vinculado.";

                    return;
                }

                lblDatosPaciente.Text = Server.HtmlEncode(
                    "Paciente vinculado: " +
                    paciente.Nombre + " " + paciente.Apellido +
                    " - IdPaciente: " + paciente.IdPaciente
                );
            }
        }



        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Seguridad.CerrarSesion(this);
        }
    }
}
