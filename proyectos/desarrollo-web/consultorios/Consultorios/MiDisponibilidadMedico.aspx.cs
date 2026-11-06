using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class MiDisponibilidadMedico : System.Web.UI.Page
    {
        private int Usuario { get { return Convert.ToInt32(Session["IdUsuario"]); } }
        private bool Acceso() { return Seguridad.RequerirRol(this,"MEDICO") && Seguridad.RequerirAcceso(this); }
        protected void Page_Init(object sender,EventArgs e) { ViewStateUserKey=Session.SessionID; }
        protected void Page_Load(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            if(!IsPostBack)
            {
                try { CargarDisponibilidad(); }
                catch(Exception ex) { pnlContenido.Visible=false; MostrarError(ex); }
            }
        }
        private void MostrarError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
            lblMensaje.Text=Server.HtmlEncode(ex is ArgumentException || ex is InvalidOperationException ? ex.Message : "No se pudo completar la operacion. Actualiza la pantalla e intenta nuevamente.");
        }
        private void CargarDisponibilidad()
        {
            var agenda=new AgendaTurnosNegocio().MiDisponibilidad(Usuario);
            gvAgenda.DataSource=agenda; gvAgenda.DataBind();
            ddlAgenda.Items.Clear(); ddlAgenda.Items.Add(new ListItem("-- Nuevo horario --","0"));
            foreach(var franja in agenda.Where(a=>a.Estado.Descripcion=="Activo"))
                ddlAgenda.Items.Add(new ListItem(franja.NombreDia+" "+franja.Horario,franja.IdAgenda.ToString()));
            gvSolicitudes.DataSource=new AgendaTurnosNegocio().SolicitudesHorarios(Usuario,false); gvSolicitudes.DataBind();
        }
        protected void ElegirAgenda(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            try
            {
                chkBaja.Checked=false;
                int id=Convert.ToInt32(ddlAgenda.SelectedValue);
                var franja=new AgendaTurnosNegocio().MiDisponibilidad(Usuario).SingleOrDefault(a=>a.IdAgenda==id);
                txtInicio.Text=franja==null?"":franja.HoraInicio.ToString(@"hh\:mm");
                txtFin.Text=franja==null?"":franja.HoraFin.ToString(@"hh\:mm");
                txtDuracion.Text=franja==null?"30":franja.DuracionMinutos.ToString();
                if(franja!=null) ddlDia.SelectedValue=franja.DiaSemana.ToString();
            }
            catch(Exception ex) { MostrarError(ex); }
        }
        protected void SolicitarHorario(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            try
            {
                TimeSpan inicio,fin; int duracion;
                if(!TimeSpan.TryParseExact(txtInicio.Text,@"hh\:mm",CultureInfo.InvariantCulture,out inicio) ||
                    !TimeSpan.TryParseExact(txtFin.Text,@"hh\:mm",CultureInfo.InvariantCulture,out fin) || !int.TryParse(txtDuracion.Text,out duracion))
                    throw new ArgumentException("Completa horas y duracion validas.");
                Agenda agenda=new Agenda {IdAgenda=int.Parse(ddlAgenda.SelectedValue),DiaSemana=int.Parse(ddlDia.SelectedValue),HoraInicio=inicio,HoraFin=fin,DuracionMinutos=duracion};
                int id=new AgendaTurnosNegocio().SolicitarHorario(Usuario,agenda,chkBaja.Checked);
                CargarDisponibilidad(); lblMensaje.Text="Solicitud de horario "+id+" enviada. La disponibilidad actual no cambio.";
            }
            catch(Exception ex) { MostrarError(ex); }
        }
    }
}
