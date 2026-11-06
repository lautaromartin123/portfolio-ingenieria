using System;
using System.Web.UI.WebControls;
using System.Linq;
using BIZ;

namespace Consultorios
{
    public partial class MisDatosMedico : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        private bool RequerirAcceso()
        {
            return Seguridad.RequerirRol(this, "MEDICO") && Seguridad.RequerirAcceso(this);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequerirAcceso()) return;
            if (!IsPostBack)
            {
                try
                {
                    CargarEspecialidades();
                    CargarTiposMatricula();
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    pnlDatos.Visible = false;
                    pnlSolicitud.Visible = false;
                    lblMensaje.Text = "No se pudieron cargar los datos del medico.";
                }
            }
        }

        private void CargarEspecialidades()
        {
            cblEspecialidades.DataSource = new EspecialidadNegocio().Listar();
            cblEspecialidades.DataTextField = "Nombre";
            cblEspecialidades.DataValueField = "IdEspecialidad";
            cblEspecialidades.DataBind();
        }

        private void CargarTiposMatricula()
        {
            ddlTipoMatricula.DataSource = new TipoMatriculaNegocio().Listar();
            ddlTipoMatricula.DataTextField = "Descripcion";
            ddlTipoMatricula.DataValueField = "IdTipoMatricula";
            ddlTipoMatricula.DataBind();
            ddlTipoMatricula.Items.Insert(0, new ListItem("-- Selecciona un tipo --", ""));
        }

        private void CargarDatos()
        {
            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            Profesional profesional = new ProfesionalNegocio().BuscarPorUsuario(idUsuario);
            if (profesional == null)
            {
                lblMensaje.Text = "Tu cuenta todavia no tiene un profesional vinculado. Consulta con un administrador.";
                return;
            }
            if (!string.Equals(profesional.Estado.Descripcion, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                lblMensaje.Text = "El profesional vinculado a tu cuenta esta inactivo.";
                return;
            }

            var especialidades = new EspecialidadNegocio().ListarPorProfesional(profesional.IdProfesional);
            var matriculas = new MatriculaNegocio().ListarPorProfesional(profesional.IdProfesional);
            litNombre.Text = profesional.Nombre;
            litApellido.Text = profesional.Apellido;
            litEspecialidad.Text = string.Join(", ", especialidades.Select(e => e.Nombre));
            txtNombre.Text = profesional.Nombre;
            txtApellido.Text = profesional.Apellido;
            foreach (ListItem item in cblEspecialidades.Items)
                item.Selected = especialidades.Any(e => e.IdEspecialidad.ToString() == item.Value);

            rptMatriculas.DataSource = matriculas;
            rptMatriculas.DataBind();
            lblSinMatricula.Visible = matriculas.Count == 0;
            pnlMatriculaInicial.Visible = matriculas.Count == 0;
            pnlDatos.Visible = true;

            int? pendiente = new SolicitudModificacionNegocio().BuscarIdPendienteMedico(idUsuario);
            pnlSolicitud.Visible = !pendiente.HasValue;
            lblMensaje.Text = pendiente.HasValue
                ? "Tu solicitud " + pendiente.Value + " esta pendiente de revision. Podras enviar otra cuando se resuelva."
                : "";
        }

        protected string MostrarValor(object valor)
        {
            string texto = Convert.ToString(valor);
            return string.IsNullOrWhiteSpace(texto) ? "NULL" : texto;
        }

        protected string MostrarFecha(object valor)
        {
            return valor == null || valor == DBNull.Value
                ? "NULL" : Convert.ToDateTime(valor).ToString("yyyy-MM-dd");
        }

        protected void btnEnviarSolicitud_Click(object sender, EventArgs e)
        {
            if (!RequerirAcceso()) return;
            lblMensaje.Text = "";
            try
            {
                SolicitudModificacionMedico datos = new SolicitudModificacionMedico();
                datos.Nombre = txtNombre.Text;
                datos.Apellido = txtApellido.Text;
                datos.IdsEspecialidades = cblEspecialidades.Items.Cast<ListItem>()
                    .Where(item => item.Selected).Select(item => int.Parse(item.Value)).ToList();

                if (pnlMatriculaInicial.Visible)
                {
                    datos.NumeroMatricula = txtNumeroMatricula.Text;
                    datos.EntidadEmisora = txtEntidadEmisora.Text;
                    if (!string.IsNullOrEmpty(ddlTipoMatricula.SelectedValue))
                        datos.IdTipoMatricula = int.Parse(ddlTipoMatricula.SelectedValue);
                    if (!string.IsNullOrWhiteSpace(txtVencimiento.Text))
                    {
                        DateTime fecha;
                        if (!DateTime.TryParseExact(txtVencimiento.Text, "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fecha))
                            throw new ArgumentException("El vencimiento no tiene una fecha valida.");
                        datos.FechaVencimiento = fecha;
                    }
                }

                int id = new SolicitudModificacionNegocio().EnviarMedico(
                    Convert.ToInt32(Session["IdUsuario"]), datos);
                lblMensaje.Text = "Solicitud " + id + " enviada. Quedo pendiente de aprobacion.";
                pnlSolicitud.Visible = false;
            }
            catch (ArgumentException ex) { lblMensaje.Text = Server.HtmlEncode(ex.Message); }
            catch (InvalidOperationException ex) { lblMensaje.Text = Server.HtmlEncode(ex.Message); }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                lblMensaje.Text = "No se pudo completar el envio. Revisa tus solicitudes antes de volver a intentarlo.";
            }
        }
    }
}
