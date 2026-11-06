using System;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class SolicitudesAgenda : System.Web.UI.Page
    {
        private int Usuario { get { return Convert.ToInt32(Session["IdUsuario"]); } }
        private bool Acceso() { return Seguridad.RequerirRol(this,"ADMIN") && Seguridad.RequerirAcceso(this); }
        protected void Page_Init(object sender,EventArgs e) { ViewStateUserKey=Session.SessionID; }
        protected void Page_Load(object sender,EventArgs e) { if(!Acceso()) return; if(!IsPostBack) Cargar(); }
        private void Cargar()
        {
            try
            {
                var tabla=new AgendaTurnosNegocio().SolicitudesHorarios(Usuario,true);
                rptSolicitudes.DataSource=tabla; rptSolicitudes.DataBind(); lblSinSolicitudes.Visible=tabla.Rows.Count==0;
            }
            catch(Exception ex) { MostrarError(ex); }
        }
        private void MostrarError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
            lblMensaje.Text=Server.HtmlEncode(ex is ArgumentException || ex is InvalidOperationException ? ex.Message : "No se pudo completar la operacion. Actualiza el listado.");
        }
        protected void Actualizar(object sender,EventArgs e) { if(!Acceso()) return; Cargar(); }
        protected void Resolver(object sender,RepeaterCommandEventArgs e)
        {
            if(!Acceso() || (e.CommandName!="Aprobar" && e.CommandName!="Rechazar")) return;
            try
            {
                string motivo=((TextBox)e.Item.FindControl("txtMotivo")).Text;
                new AgendaTurnosNegocio().ResolverHorario(Usuario,Convert.ToInt32(e.CommandArgument),e.CommandName=="Aprobar",motivo);
                lblMensaje.Text=e.CommandName=="Aprobar"?"Horario aprobado. Disponibilidad actualizada.":"Solicitud de horario rechazada.";
                Cargar();
            }
            catch(Exception ex) { MostrarError(ex); }
        }
    }
}
