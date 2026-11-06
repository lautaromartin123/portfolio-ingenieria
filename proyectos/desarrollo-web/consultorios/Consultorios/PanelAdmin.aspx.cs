using System;

namespace Consultorios
{
    public partial class PanelAdmin :
        System.Web.UI.Page
    {
        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // Este panel pertenece exclusivamente
            // al nivel ADMIN.

            if (!Seguridad.RequerirRol(
                this,
                "ADMIN"))
            {
                return;
            }


            // Ademas debe tener acceso a la URL
            // mediante Nivel_URL.

            if (!Seguridad.RequerirAcceso(this))
            {
                return;
            }


            if (!IsPostBack)
            {
                MostrarUsuario();

                ConfigurarOpciones();
            }
        }


        // =====================================================
        // MOSTRAR USUARIO
        // =====================================================

        private void MostrarUsuario()
        {
            lblUsuario.Text =
                "Admin: "
                +
                Convert.ToString(
                    Session["Nombre"])
                +
                " "
                +
                Convert.ToString(
                    Session["Apellido"]);
        }


        // =====================================================
        // COMPROBAR ACCESO A UNA PAGINA
        // =====================================================

        private bool TieneAccesoPagina(
            string pagina)
        {
            if (Session["IdNivel"] == null)
            {
                return false;
            }


            int idNivel =
                Convert.ToInt32(
                    Session["IdNivel"]);


            UsuarioDAO dao =
                new UsuarioDAO();


            return dao.TieneAcceso(
                idNivel,
                pagina);
        }


        // =====================================================
        // CONFIGURAR OPCIONES DEL PANEL
        // =====================================================

        private void ConfigurarOpciones()
        {
            lnkSolicitudesAgenda.Visible = TieneAccesoPagina("SolicitudesAgenda.aspx");
            // =============================================
            // PROFESIONALES
            // =============================================

            // Todavia no existe un permiso funcional
            // especifico para profesionales.
            // Por ahora depende solamente de Nivel_URL.

            lnkProfesionales.Visible =
                TieneAccesoPagina(
                    "ListadoProfesionales.aspx");


            // =============================================
            // AGENDA
            // =============================================

            lnkAgenda.Visible =
                TieneAccesoPagina(
                    "AgendaProfesional.aspx")
                &&
                Seguridad.TienePermiso(
                    "AGENDA_VER");


            // =============================================
            // TURNOS
            // =============================================

            lnkTurnos.Visible =
                TieneAccesoPagina(
                    "ListadoTurnos.aspx")
                &&
                Seguridad.TienePermiso(
                    "TURNOS_VER");


            // =============================================
            // GESTION DE USUARIOS
            // =============================================

            lnkGestionUsuarios.Visible =
                TieneAccesoPagina(
                    "GestionUsuarios.aspx")
                &&
                Seguridad.TienePermiso(
                    "USUARIOS_GESTIONAR");


            // =============================================
            // GESTION DE PERMISOS
            // =============================================

            lnkGestionPermisos.Visible =
                TieneAccesoPagina(
                    "GestionPermisos.aspx")
                &&
                Seguridad.TienePermiso(
                    "PERMISOS_GESTIONAR");


            // =============================================
            // SOLICITUDES DE MODIFICACION
            // =============================================

            // Todavia no existe un permiso funcional
            // especifico para esta operacion.

            lnkSolicitudesModificacion.Visible =
                TieneAccesoPagina(
                    "SolicitudesModificacion.aspx");


            // =============================================
            // OCULTAR SECCIONES VACIAS
            // =============================================

            pnlProfesionalesTurnos.Visible =
                lnkProfesionales.Visible
                ||
                lnkAgenda.Visible
                ||
                lnkTurnos.Visible;


            pnlGestion.Visible =
                lnkGestionUsuarios.Visible
                ||
                lnkGestionPermisos.Visible
                ||
                lnkSolicitudesModificacion.Visible;
        }


        // =====================================================
        // CERRAR SESION
        // =====================================================

        protected void btnCerrarSesion_Click(
            object sender,
            EventArgs e)
        {
            Seguridad.CerrarSesion(
                this);
        }
    }
}
