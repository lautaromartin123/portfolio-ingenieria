using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class MisTurnosPaciente : System.Web.UI.Page
    {
        private int Usuario { get { return Convert.ToInt32(Session["IdUsuario"]); } }
        private bool Historial { get { return Request.QueryString["vista"]=="historial"; } }
        private bool Acceso() { return Seguridad.RequerirRol(this,"PACIENTE") && Seguridad.RequerirAcceso(this); }
        protected void Page_Init(object sender,EventArgs e) { ViewStateUserKey=Session.SessionID; }
        protected void Page_Load(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            if(!IsPostBack)
            {
                try
                {
                    litTitulo.Text=Historial?"Historial de atenciones realizadas":"Mi agenda y proximos turnos";
                    pnlReserva.Visible=!Historial;
                    CargarTurnos();
                    if(!Historial)
                    {
                        DataRow cobertura=new AgendaTurnosNegocio().CoberturaPaciente(Usuario).Rows[0];
                        litCobertura.Text="Cobertura actual: "+cobertura["ObraSocial"]+" - Afiliado: "+cobertura["Afiliado"];
                        ddlProfesional.DataSource=new AgendaTurnosNegocio().ProfesionalesDisponibles(Usuario);
                        ddlProfesional.DataTextField="Nombre"; ddlProfesional.DataValueField="IdProfesional"; ddlProfesional.DataBind();
                        ddlProfesional.Items.Insert(0,new ListItem("-- Selecciona un medico --",""));
                        txtFecha.Text=AgendaTurnosNegocio.Ahora().Date.AddDays(1).ToString("yyyy-MM-dd");
                    }
                }
                catch(Exception ex) { pnlContenido.Visible=false; MostrarError(ex); }
            }
        }
        private void MostrarError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
            lblMensaje.Text=Server.HtmlEncode(ex is ArgumentException || ex is InvalidOperationException ? ex.Message : "No se pudo completar la operacion. Actualiza la pantalla e intenta nuevamente.");
        }
        private void CargarTurnos()
        {
            gvTurnos.DataSource=new AgendaTurnosNegocio().MisTurnos(Usuario,false,Historial); gvTurnos.DataBind();
        }
        protected void CambiarProfesional(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            ddlHorario.Items.Clear(); litEspecialidades.Text="";
            try
            {
                var fila=new AgendaTurnosNegocio().ProfesionalesDisponibles(Usuario).AsEnumerable()
                    .SingleOrDefault(r=>Convert.ToString(r["IdProfesional"])==ddlProfesional.SelectedValue);
                if(fila!=null) litEspecialidades.Text="Especialidades: "+fila["Especialidades"];
            }
            catch(Exception ex) { MostrarError(ex); }
        }
        protected void BuscarHorarios(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            ddlHorario.Items.Clear();
            try
            {
                DateTime fecha; int profesional;
                if(!DateTime.TryParseExact(txtFecha.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out fecha)
                    || !int.TryParse(ddlProfesional.SelectedValue,out profesional)) throw new ArgumentException("Selecciona medico y fecha.");
                var horarios=new AgendaTurnosNegocio().HorariosLibres(Usuario,false,profesional,fecha);
                foreach(DataRow fila in horarios.Rows)
                    ddlHorario.Items.Add(new ListItem(Convert.ToString(fila["Horario"]),Convert.ToDateTime(fila["FechaHora"]).ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture)));
                lblMensaje.Text=horarios.Rows.Count==0?"No hay horarios libres para ese medico y fecha.":"Selecciona el horario para solicitar el turno.";
            }
            catch(Exception ex) { MostrarError(ex); }
        }
        protected void Solicitar(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            try
            {
                DateTime fecha; int profesional;
                if(!DateTime.TryParseExact(ddlHorario.SelectedValue,"yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None,out fecha)
                    || !int.TryParse(ddlProfesional.SelectedValue,out profesional)) throw new ArgumentException("Busca y selecciona un horario disponible.");
                DateTime dia;
                if(!DateTime.TryParseExact(txtFecha.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out dia) || dia!=fecha.Date)
                    throw new ArgumentException("La fecha cambio. Busca los horarios nuevamente.");
                int id=new AgendaTurnosNegocio().Reservar(Usuario,false,profesional,0,fecha,txtMotivo.Text);
                ddlHorario.Items.Clear(); CargarTurnos(); lblMensaje.Text="Turno "+id+" solicitado. Estado: Pendiente.";
            }
            catch(Exception ex) { MostrarError(ex); }
        }
    }
}
