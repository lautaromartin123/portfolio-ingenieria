using System;
using BIZ;

namespace Consultorios
{
    public partial class DetalleTurnoMedico : System.Web.UI.Page
    {
        private int Usuario { get { return Convert.ToInt32(Session["IdUsuario"]); } }
        private int IdTurno
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"],out id) && id>0 ? id : 0;
            }
        }
        private bool Acceso()
        {
            return Seguridad.RequerirRol(this,"MEDICO") && Seguridad.RequerirAcceso(this);
        }
        protected void Page_Init(object sender,EventArgs e)
        {
            // Evita usar el formulario de un turno para guardar en otro.
            ViewStateUserKey=Session.SessionID+":"+IdTurno;
        }
        protected void Page_Load(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            if(!IsPostBack) CargarTurno();
        }
        private string Mostrar(object valor)
        {
            return string.IsNullOrWhiteSpace(Convert.ToString(valor))?"NULL":Convert.ToString(valor);
        }
        private void MostrarError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
            lblMensaje.Text=Server.HtmlEncode(ex is ArgumentException || ex is InvalidOperationException
                ? ex.Message : "No se pudo completar la operacion. Actualiza el turno e intenta nuevamente.");
        }
        private void CargarTurno()
        {
            try
            {
                var tabla=new AgendaTurnosNegocio().BuscarTurnoMedico(Usuario,IdTurno);
                if(tabla.Rows.Count!=1)
                    throw new InvalidOperationException("El turno no existe o no pertenece a tu agenda.");
                var fila=tabla.Rows[0];
                DateTime fecha=Convert.ToDateTime(fila["FechaHora"]);
                string estado=Convert.ToString(fila["Estado"]);
                litIdTurno.Text=IdTurno.ToString();
                litFecha.Text=fecha.ToString("dd/MM/yyyy HH:mm");
                litDuracion.Text=Convert.ToString(fila["DuracionMinutos"]);
                litEstado.Text=estado;
                litMotivo.Text=Mostrar(fila["Motivo"]);
                litPaciente.Text=fila["Apellido"]+", "+fila["Nombre"];
                litIdPaciente.Text=Convert.ToString(fila["IdPaciente"]);
                litTelefono.Text=Mostrar(fila["Telefono"]);
                litCobertura.Text=fila["ObraSocial"]+" ("+fila["TipoCobertura"]+")";
                litAfiliado.Text=Mostrar(fila["Afiliado"]);
                litCoberturaActual.Text=Convert.ToString(fila["CoberturaActual"]);
                txtObservacion.Text=Convert.ToString(fila["Observaciones"]);
                bool cerrado=estado=="Atendido" || estado=="Ausente";
                pnlRegistro.Visible=!cerrado;
                pnlResultadoGuardado.Visible=cerrado;
                litObservacionGuardada.Text=Mostrar(fila["Observaciones"]);
                btnActualizar.Visible=!cerrado;
                lnkVolver.NavigateUrl=cerrado?"~/MiAgendaMedico.aspx?vista=historial":"~/MiAgendaMedico.aspx";
                lnkVolver.Text=cerrado?"Volver al historial de turnos":"Volver a mi agenda";
                rblResultado.ClearSelection();
                foreach(System.Web.UI.WebControls.ListItem item in rblResultado.Items)
                {
                    item.Selected=item.Value==estado;
                    item.Enabled=(estado!="Atendido" && estado!="Ausente") || item.Value==estado;
                }
                bool habilitado=fecha<=AgendaTurnosNegocio.Ahora() && (estado=="Pendiente" || estado=="Confirmado");
                rblResultado.Enabled=habilitado;
                txtObservacion.ReadOnly=!habilitado;
                btnGuardar.Enabled=habilitado;
                if(estado=="Cancelado") lblAviso.Text="El turno esta cancelado. No se puede registrar una atencion.";
                else if(!habilitado) lblAviso.Text="Podras guardar el resultado desde el inicio del turno. Si ya llega la hora, toca Actualizar turno.";
                else lblAviso.Text="Selecciona el resultado y completa las observaciones antes de guardar.";
                pnlContenido.Visible=true;
            }
            catch(Exception ex) { pnlContenido.Visible=false; MostrarError(ex); }
        }
        protected void Actualizar(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            lblMensaje.Text="";
            CargarTurno();
        }
        protected void GuardarResultado(object sender,EventArgs e)
        {
            if(!Acceso()) return;
            try
            {
                if(rblResultado.SelectedValue!="Atendido" && rblResultado.SelectedValue!="Ausente")
                    throw new ArgumentException("Indica si el turno se realizo o no.");
                bool realizado=rblResultado.SelectedValue=="Atendido";
                new AgendaTurnosNegocio().RegistrarResultadoAtencion(Usuario,IdTurno,txtObservacion.Text,realizado);
                Session["MensajeTurno"]=realizado?"Turno Atendido (OK). Resultado guardado y cerrado. Las observaciones estan disponibles en el historial del paciente."
                    :"Turno marcado como Ausente. Resultado guardado y cerrado.";
                Response.Redirect(ResolveUrl("~/MiAgendaMedico.aspx?vista=historial"),false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            catch(InvalidOperationException ex) { CargarTurno(); MostrarError(ex); }
            catch(Exception ex) { MostrarError(ex); }
        }
    }
}
