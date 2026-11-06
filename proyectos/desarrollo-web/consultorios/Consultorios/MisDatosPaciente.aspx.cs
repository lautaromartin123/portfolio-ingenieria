using BIZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Consultorios
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            if (!IsPostBack)
            {
                CargarObrasSociales();
                CargarDatos();
            }
        }


        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }




        private void CargarDatos()
        {
            int idUsuario =
                Convert.ToInt32(Session["IdUsuario"]);

            PacienteNegocio negocio = new PacienteNegocio();

            Paciente paciente =
                negocio.BuscarPorUsuario(idUsuario);

            if (paciente == null)
            {
                lblMensaje.Text =
                    "Tu cuenta todavía no tiene un paciente vinculado.";

                return;
            }

            litNombre.Text = paciente.Nombre;
            litApellido.Text = paciente.Apellido;

            litTelefono.Text =
                MostrarTexto(paciente.Telefono);

            litObraSocial.Text =
                paciente.ObraSocial == null
                    ? "Sin obra social registrada"
                    : paciente.ObraSocial.Nombre;

            litNumeroObraSocial.Text =
                MostrarTexto(paciente.NumeroObraSocial);

            txtNombre.Text = paciente.Nombre;
            txtApellido.Text = paciente.Apellido;
            txtTelefono.Text = paciente.Telefono;
            txtNumeroObraSocial.Text = paciente.NumeroObraSocial;

            ddlObraSocial.SelectedValue =
                paciente.IdObraSocial.HasValue
                    ? paciente.IdObraSocial.Value.ToString()
                    : "";



            pnlDatos.Visible = true;


            SolicitudModificacionNegocio solicitudes =
               new SolicitudModificacionNegocio();

            int? idPendiente =
                solicitudes.BuscarIdPendientePaciente(idUsuario);

            pnlSolicitud.Visible = !idPendiente.HasValue;

            if (idPendiente.HasValue)
            {
                lblMensaje.Text =
                    "Tu solicitud " + idPendiente.Value +
                    " está pendiente de revisión. " +
                    "Podrás enviar otra cuando se resuelva.";
            }
            else
            {
                lblMensaje.Text = "";
            }



        }





        private string MostrarTexto(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "Sin informar";

            return valor;
        }


        private void CargarObrasSociales()
        {
            ObraSocialNegocio negocio = new ObraSocialNegocio();

            ddlObraSocial.DataSource = negocio.Listar();
            ddlObraSocial.DataTextField = "Nombre";
            ddlObraSocial.DataValueField = "IdObraSocial";
            ddlObraSocial.DataBind();

            ddlObraSocial.Items.Insert(
                0,
                new ListItem("-- Sin obra social --", "")
            );
        }


        protected void btnEnviarSolicitud_Click(object sender, EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            lblMensaje.Text = "";

            try
            {
                int idUsuario =
                    Convert.ToInt32(Session["IdUsuario"]);

                SolicitudModificacionPaciente datos =
                    new SolicitudModificacionPaciente();

                datos.Nombre = txtNombre.Text;
                datos.Apellido = txtApellido.Text;
                datos.Telefono = txtTelefono.Text;
                datos.NumeroObraSocial = txtNumeroObraSocial.Text;

                if (string.IsNullOrEmpty(ddlObraSocial.SelectedValue))
                {
                    datos.IdObraSocial = null;
                }
                else
                {
                    int idObraSocial;

                    if (!int.TryParse(
                        ddlObraSocial.SelectedValue,
                        out idObraSocial))
                    {
                        lblMensaje.Text = "La obra social seleccionada no es válida.";
                        return;
                    }

                    datos.IdObraSocial = idObraSocial;
                }

                SolicitudModificacionNegocio negocio =
                    new SolicitudModificacionNegocio();

                int idSolicitud =
                    negocio.EnviarPaciente(idUsuario, datos);

                lblMensaje.Text =
                    "Solicitud " + idSolicitud +
                    " enviada. Quedó pendiente de aprobación.";

                pnlSolicitud.Visible = false;
            }
            catch (ArgumentException ex)
            {
                lblMensaje.Text = Server.HtmlEncode(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                lblMensaje.Text = Server.HtmlEncode(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());

                lblMensaje.Text =
                    "No se pudo completar el envío. Revisá el estado de tus " +
                    "solicitudes antes de volver a intentarlo.";
            }
        }










    }
}
