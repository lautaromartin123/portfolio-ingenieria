using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using BIZ;



namespace Consultorios
{
    public partial class SolicitudesModificacion : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            if (!IsPostBack)
            {
                CargarSolicitudes();
            }
        }

        private void CargarSolicitudes()
        {
            lblMensaje.Text = "";
            rptSolicitudes.Visible = false;
            rptSolicitudesMedico.Visible = false;
            lblSinSolicitudesPaciente.Visible = false;
            lblSinSolicitudesMedico.Visible = false;

            try
            {
                int idUsuarioAdmin =
                    Convert.ToInt32(Session["IdUsuario"]);

                SolicitudModificacionNegocio negocio =
                    new SolicitudModificacionNegocio();

                DataTable tabla =
                    negocio.ListarPendientesPaciente(idUsuarioAdmin);

                DataTable tablaMedicos =
                    negocio.ListarPendientesMedico(idUsuarioAdmin);

                rptSolicitudes.DataSource = tabla;
                rptSolicitudes.DataBind();

                rptSolicitudesMedico.DataSource = tablaMedicos;
                rptSolicitudesMedico.DataBind();

                rptSolicitudes.Visible = tabla.Rows.Count > 0;
                rptSolicitudesMedico.Visible = tablaMedicos.Rows.Count > 0;
                lblSinSolicitudesPaciente.Visible = tabla.Rows.Count == 0;
                lblSinSolicitudesMedico.Visible = tablaMedicos.Rows.Count == 0;
            }
            catch (InvalidOperationException ex)
            {
                lblMensaje.Text = Server.HtmlEncode(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());

                lblMensaje.Text =
                    "No se pudieron cargar las solicitudes.";
            }
        }

        protected string MostrarValor(object valor)
        {
            string texto = Convert.ToString(valor);

            if (string.IsNullOrWhiteSpace(texto))
                return "Sin informar";

            return texto;
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            CargarSolicitudes();
        }


        protected void rptSolicitudes_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            ResolverSolicitud(e, false);
        }

        protected void rptSolicitudesMedico_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            ResolverSolicitud(e, true);
        }

        private void ResolverSolicitud(RepeaterCommandEventArgs e, bool esMedico)
        {
            if (!Seguridad.RequerirAcceso(this))
                return;

            if (e.CommandName != "Rechazar" &&
                e.CommandName != "Aprobar")
            {
                return;
            }

            lblMensaje.Text = "";

            int idSolicitud;

            if (!int.TryParse(
                Convert.ToString(e.CommandArgument),
                out idSolicitud) || idSolicitud <= 0)
            {
                lblMensaje.Text = "No se pudo identificar la solicitud.";
                return;
            }

            try
            {
                int idUsuarioAdmin =
                    Convert.ToInt32(Session["IdUsuario"]);

                SolicitudModificacionNegocio negocio =
                    new SolicitudModificacionNegocio();

                string mensaje;

                if (e.CommandName == "Aprobar")
                {
                    if (esMedico)
                        negocio.AprobarMedico(idSolicitud, idUsuarioAdmin);
                    else
                        negocio.AprobarPaciente(idSolicitud, idUsuarioAdmin);

                    mensaje =
                        "Solicitud aprobada correctamente. " +
                        (esMedico
                            ? "Se actualizaron los datos del medico."
                            : "Se actualizaron los datos del paciente.");
                }
                else
                {
                    TextBox txtObservacion =
                        (TextBox)e.Item.FindControl("txtObservacion");

                    if (esMedico)
                        negocio.RechazarMedico(idSolicitud, idUsuarioAdmin, txtObservacion.Text);
                    else
                        negocio.RechazarPaciente(idSolicitud, idUsuarioAdmin, txtObservacion.Text);

                    mensaje = "Solicitud rechazada correctamente.";
                }

                CargarSolicitudes();

                lblMensaje.Text = mensaje + " " + lblMensaje.Text;
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
                    "No se pudo confirmar la operacion. " +
                    "Actualiza el listado para comprobar el estado.";
            }
        }











    }
}
