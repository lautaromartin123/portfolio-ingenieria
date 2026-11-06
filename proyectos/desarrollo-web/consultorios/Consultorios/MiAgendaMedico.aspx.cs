using System;
using System.Data;
using System.Globalization;
using System.Web.UI.WebControls;
using BIZ;

namespace Consultorios
{
    public partial class MiAgendaMedico : System.Web.UI.Page
    {
        private int Usuario { get { return Convert.ToInt32(Session["IdUsuario"]); } }
        private bool EsHistorial { get { return Request.QueryString["vista"]=="historial"; } }
        private bool Acceso() { return Seguridad.RequerirRol(this,"MEDICO") && Seguridad.RequerirAcceso(this); }
        protected void Page_Init(object sender,EventArgs e) { ViewStateUserKey=Session.SessionID; }
        protected void Page_Load(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            pnlNuevoTurno.Visible=!EsHistorial;
            if(!IsPostBack)
            {
                try
                {
                    litTitulo.Text=EsHistorial?"Historial de turnos del medico":"Mi agenda de medico";
                    litListado.Text=EsHistorial?"Turnos cerrados (solo lectura)":"Turnos de mi agenda";
                    btnTodos.Text=EsHistorial?"Ver todo el historial":"Ver todos mis turnos";
                    txtFiltro.Text=EsHistorial?"":AgendaTurnosNegocio.Ahora().ToString("yyyy-MM-dd");
                    if(!EsHistorial)
                    {
                        txtFecha.Text=AgendaTurnosNegocio.Ahora().Date.AddDays(1).ToString("yyyy-MM-dd");
                        ddlPaciente.DataSource=new AgendaTurnosNegocio().PacientesParaTurno(Usuario);
                        ddlPaciente.DataTextField="Nombre"; ddlPaciente.DataValueField="IdPaciente"; ddlPaciente.DataBind();
                        ddlPaciente.Items.Insert(0,new ListItem("-- Selecciona un paciente --",""));
                    }
                    CargarTurnos();
                    if(Session["MensajeTurno"]!=null)
                    {
                        lblMensaje.Text=Server.HtmlEncode(Convert.ToString(Session["MensajeTurno"]));
                        Session.Remove("MensajeTurno");
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
            DateTime? fecha=string.IsNullOrEmpty(txtFiltro.Text)?(DateTime?)null:Fecha(txtFiltro.Text);
            foreach(DataControlField columna in gvTurnos.Columns)
                if(columna is BoundField && ((BoundField)columna).DataField=="Observaciones")
                    columna.Visible=EsHistorial;
            gvTurnos.DataSource=new AgendaTurnosNegocio().MisTurnos(Usuario,true,EsHistorial,fecha); gvTurnos.DataBind();
        }
        private DateTime Fecha(string texto)
        {
            DateTime fecha;
            if(!DateTime.TryParseExact(texto,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out fecha))
                throw new ArgumentException("Selecciona una fecha valida.");
            return fecha;
        }
        protected void Filtrar(object sender,EventArgs e) { if(!Acceso()) return; try { CargarTurnos(); } catch(Exception ex) { MostrarError(ex); } }
        protected void VerTodos(object sender,EventArgs e) { if(!Acceso()) return; txtFiltro.Text=""; try { CargarTurnos(); } catch(Exception ex) { MostrarError(ex); } }
        protected void BuscarHorarios(object sender,EventArgs e)
        {
            if(!Acceso() || EsHistorial) return;
            try
            {
                ddlTurno.Items.Clear();
                var horarios=new AgendaTurnosNegocio().HorariosLibres(Usuario,true,0,Fecha(txtFecha.Text));
                foreach(DataRow fila in horarios.Rows)
                    ddlTurno.Items.Add(new ListItem(Convert.ToString(fila["Horario"]),Convert.ToDateTime(fila["FechaHora"]).ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture)));
                lblMensaje.Text=horarios.Rows.Count==0?"No hay horarios libres para ese dia.":"Selecciona el horario y agrega el turno.";
            }
            catch(Exception ex) { MostrarError(ex); }
        }
        protected void Reservar(object sender,EventArgs e)
        {
            if(!Acceso() || EsHistorial) return;
            try
            {
                DateTime fecha; int paciente;
                if(!DateTime.TryParseExact(ddlTurno.SelectedValue,"yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None,out fecha) || !int.TryParse(ddlPaciente.SelectedValue,out paciente))
                    throw new ArgumentException("Selecciona paciente, fecha y un horario disponible.");
                if(fecha.Date!=Fecha(txtFecha.Text)) throw new ArgumentException("La fecha cambio. Busca los horarios nuevamente.");
                int id=new AgendaTurnosNegocio().Reservar(Usuario,true,0,paciente,fecha,txtMotivo.Text);
                txtFiltro.Text=fecha.ToString("yyyy-MM-dd");
                ddlTurno.Items.Clear(); CargarTurnos(); lblMensaje.Text="Turno "+id+" agregado como Pendiente.";
            }
            catch(Exception ex) { MostrarError(ex); }
        }
    }
}
