using System;

namespace Consultorios
{
    public partial class PanelMedico :
        System.Web.UI.Page
    {
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!Seguridad.RequerirRol(
                this,
                "MEDICO"))
            {
                return;
            }

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


        private void MostrarUsuario()
        {
            lblUsuario.Text =
                "Médico: "
                +
                Convert.ToString(
                    Session["Nombre"])
                +
                " "
                +
                Convert.ToString(
                    Session["Apellido"]);
        }


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


        private void ConfigurarOpciones()
        {
            // AGENDA

            lnkAgenda.Visible =
                TieneAccesoPagina(
                    "AgendaProfesional.aspx")
                &&
                Seguridad.TienePermiso(
                    "AGENDA_VER");


            // TURNOS

            lnkTurnos.Visible =
                TieneAccesoPagina(
                    "ListadoTurnos.aspx")
                &&
                Seguridad.TienePermiso(
                    "TURNOS_VER");


            // IA

            btnIA.Visible =
                TieneAccesoPagina(
                    "PanelIAMedico.aspx");


            bool tieneOpciones =
                lnkAgenda.Visible
                ||
                lnkTurnos.Visible
                ||
                btnIA.Visible;


            pnlFunciones.Visible =
                tieneOpciones;

            lblSinOpciones.Visible =
                !tieneOpciones;
        }


        // =====================================================
        // ABRIR PANEL DE IA
        // =====================================================

        protected void BtnIA_Click(
            object sender,
            EventArgs e)
        {
            Response.Redirect(
                "~/PanelIAMedico.aspx");
        }


        // =====================================================
        // CERRAR SESIÓN
        // =====================================================

        protected void BtnCerrarSesion_Click(
            object sender,
            EventArgs e)
        {
            Seguridad.CerrarSesion(this);
        }
    }
}
