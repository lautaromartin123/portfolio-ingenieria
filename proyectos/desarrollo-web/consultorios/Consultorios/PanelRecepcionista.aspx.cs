using System;

namespace Consultorios
{
    public partial class PanelRecepcionista :
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
            // al nivel RECEPCIONISTA.

            if (!Seguridad.RequerirRol(
                this,
                "RECEPCIONISTA"))
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
                "Recepcionista: "
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
        // COMPROBAR ACCESO A UNA PÁGINA
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
        // CONFIGURAR OPCIONES
        // =====================================================

        private void ConfigurarOpciones()
        {
            // =============================================
            // AGENDA
            // =============================================

            bool puedeVerAgenda =
                TieneAccesoPagina(
                    "AgendaProfesional.aspx")
                &&
                Seguridad.TienePermiso(
                    "AGENDA_VER");


            // =============================================
            // TURNOS
            // =============================================

            bool puedeVerTurnos =
                TieneAccesoPagina(
                    "ListadoTurnos.aspx")
                &&
                Seguridad.TienePermiso(
                    "TURNOS_VER");


            // =============================================
            // MOSTRAR / OCULTAR
            // =============================================

            lnkAgenda.Visible =
                puedeVerAgenda;


            pnlAgenda.Visible =
                puedeVerAgenda;


            lnkTurnos.Visible =
                puedeVerTurnos;


            pnlTurnos.Visible =
                puedeVerTurnos;


            lblSinOpciones.Visible =
                !puedeVerAgenda
                &&
                !puedeVerTurnos;
        }


        // =====================================================
        // CERRAR SESIÓN
        // =====================================================

        protected void BtnCerrarSesion_Click(
            object sender,
            EventArgs e)
        {
            Seguridad.CerrarSesion(
                this);
        }
    }
}
