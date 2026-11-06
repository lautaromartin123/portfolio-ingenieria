using System;
using System.Data;

namespace Consultorios
{
    public partial class GestionUsuarios :
        System.Web.UI.Page
    {
        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
            {
                return;
            }


            if (!Seguridad.RequerirPermiso(
                this,
                "USUARIOS_GESTIONAR"))
            {
                return;
            }


            if (!IsPostBack)
            {
                CargarUsuarios();

                CargarNiveles();

                CargarEstados();


                if (ddlUsuarios.Items.Count > 0)
                {
                    CargarUsuarioSeleccionado();
                }
            }
        }


        // =====================================================
        // SABER SI ES EL USUARIO DE LA SESIÓN ACTUAL
        // =====================================================

        private bool EsUsuarioActual(
            int idUsuario)
        {
            if (Session["IdUsuario"] == null)
            {
                return false;
            }


            int idUsuarioActual =
                Convert.ToInt32(
                    Session["IdUsuario"]);


            return idUsuario ==
                idUsuarioActual;
        }


        // =====================================================
        // CARGAR USUARIOS
        // =====================================================

        private void CargarUsuarios()
        {
            UsuarioDAO dao =
                new UsuarioDAO();


            ddlUsuarios.DataSource =
                dao.ObtenerUsuarios();


            ddlUsuarios.DataTextField =
                "Email";


            ddlUsuarios.DataValueField =
                "IdUsuario";


            ddlUsuarios.DataBind();
        }


        // =====================================================
        // CARGAR NIVELES
        // =====================================================

        private void CargarNiveles()
        {
            UsuarioDAO dao =
                new UsuarioDAO();


            ddlNivel.DataSource =
                dao.ObtenerNiveles();


            ddlNivel.DataTextField =
                "Descripcion";


            ddlNivel.DataValueField =
                "IdNivel";


            ddlNivel.DataBind();
        }


        // =====================================================
        // CARGAR ESTADOS
        // =====================================================

        private void CargarEstados()
        {
            UsuarioDAO dao =
                new UsuarioDAO();


            ddlEstado.DataSource =
                dao.ObtenerEstados();


            ddlEstado.DataTextField =
                "Descripcion";


            ddlEstado.DataValueField =
                "IdEstado";


            ddlEstado.DataBind();
        }


        // =====================================================
        // CARGAR USUARIO SELECCIONADO
        // =====================================================

        private void CargarUsuarioSeleccionado()
        {
            if (ddlUsuarios.Items.Count == 0)
            {
                return;
            }


            int idUsuario =
                Convert.ToInt32(
                    ddlUsuarios.SelectedValue);


            UsuarioDAO dao =
                new UsuarioDAO();


            DataRow fila =
                dao.ObtenerUsuarioPorId(
                    idUsuario);


            if (fila == null)
            {
                lblMensaje.Text =
                    "No se pudo cargar el usuario.";

                return;
            }


            lblNombre.Text =
                "Nombre: "
                +
                fila["Nombre"].ToString();


            lblApellido.Text =
                "Apellido: "
                +
                fila["Apellido"].ToString();


            lblEmail.Text =
                "Gmail: "
                +
                fila["Email"].ToString();


            ddlNivel.SelectedValue =
                fila["IdNivel"].ToString();


            ddlEstado.SelectedValue =
                fila["IdEstado"].ToString();


            ConfigurarProteccionUsuarioActual(
                idUsuario);
        }


        // =====================================================
        // PROTEGER AL USUARIO ACTUAL
        // =====================================================

        private void ConfigurarProteccionUsuarioActual(
            int idUsuario)
        {
            bool esUsuarioActual =
                EsUsuarioActual(
                    idUsuario);


            if (esUsuarioActual)
            {
                // Se muestran sus datos,
                // pero no puede cambiarse a sí mismo
                // de nivel ni de estado.

                ddlNivel.Enabled =
                    false;


                ddlEstado.Enabled =
                    false;


                btnGuardar.Enabled =
                    false;


                lblMensaje.Text =
                    "Por seguridad, no puede modificar "
                    +
                    "su propio nivel o estado desde esta pantalla.";
            }
            else
            {
                ddlNivel.Enabled =
                    true;


                ddlEstado.Enabled =
                    true;


                btnGuardar.Enabled =
                    true;
            }
        }


        // =====================================================
        // CAMBIAR USUARIO SELECCIONADO
        // =====================================================

        protected void DdlUsuarios_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            lblMensaje.Text =
                "";


            CargarUsuarioSeleccionado();
        }


        // =====================================================
        // GUARDAR CAMBIOS
        // =====================================================

        protected void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            // =============================================
            // PERMISO FUNCIONAL
            // =============================================

            if (!Seguridad.TienePermiso(
                "USUARIOS_GESTIONAR"))
            {
                lblMensaje.Text =
                    "No tiene permiso para gestionar usuarios.";

                return;
            }


            if (ddlUsuarios.Items.Count == 0)
            {
                lblMensaje.Text =
                    "No hay usuarios disponibles.";

                return;
            }


            try
            {
                int idUsuario =
                    Convert.ToInt32(
                        ddlUsuarios.SelectedValue);


                // =========================================
                // PROTECCIÓN DEL USUARIO ACTUAL
                // =========================================

                // Esta validación es del lado servidor.
                // Aunque alguien manipule el HTML,
                // no puede cambiar su propia cuenta.

                if (EsUsuarioActual(
                    idUsuario))
                {
                    lblMensaje.Text =
                        "Por seguridad, no puede modificar "
                        +
                        "su propio nivel o estado.";

                    return;
                }


                int idNivel =
                    Convert.ToInt32(
                        ddlNivel.SelectedValue);


                int idEstado =
                    Convert.ToInt32(
                        ddlEstado.SelectedValue);


                // =========================================
                // SEGUNDA VALIDACIÓN DE PERMISO
                // =========================================

                if (!Seguridad.TienePermiso(
                    "USUARIOS_GESTIONAR"))
                {
                    lblMensaje.Text =
                        "No tiene permiso para gestionar usuarios.";

                    return;
                }


                UsuarioDAO dao =
                    new UsuarioDAO();


                dao.ActualizarNivelEstado(
                    idUsuario,
                    idNivel,
                    idEstado);


                lblMensaje.Text =
                    "Usuario actualizado correctamente.";


                // =========================================
                // RECARGAR
                // =========================================

                CargarUsuarios();


                if (ddlUsuarios.Items.FindByValue(
                    idUsuario.ToString()) != null)
                {
                    ddlUsuarios.SelectedValue =
                        idUsuario.ToString();
                }


                CargarUsuarioSeleccionado();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "Error: "
                    +
                    ex.Message;
            }
        }
    }
}
