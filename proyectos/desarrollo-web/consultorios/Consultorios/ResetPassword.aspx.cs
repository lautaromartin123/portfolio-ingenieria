using System;

namespace Consultorios
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ValidarToken();
            }
        }


        private void ValidarToken()
        {
            string token =
                Request.QueryString["token"];


            if (string.IsNullOrEmpty(token))
            {
                lblMensaje.Text =
                    "El enlace de recuperación no es válido.";

                btnCambiar.Enabled =
                    false;

                return;
            }


            try
            {
                UsuarioDAO dao =
                    new UsuarioDAO();


                int idUsuario =
                    dao.ObtenerUsuarioPorToken(token);


                if (idUsuario == 0)
                {
                    lblMensaje.Text =
                        "El enlace venció o ya fue utilizado.";

                    btnCambiar.Enabled =
                        false;
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "Error: " + ex.Message;

                btnCambiar.Enabled =
                    false;
            }
        }


        protected void BtnCambiar_Click(
            object sender,
            EventArgs e)
        {
            string token =
                Request.QueryString["token"];


            if (string.IsNullOrEmpty(token))
            {
                lblMensaje.Text =
                    "El enlace de recuperación no es válido.";

                return;
            }


            string nuevaPassword =
                txtNuevaPassword.Text;

            string confirmarPassword =
                txtConfirmarPassword.Text;


            if (nuevaPassword == "" ||
                confirmarPassword == "")
            {
                lblMensaje.Text =
                    "Debe completar ambos campos.";

                return;
            }


            // MÍNIMO 6 CARACTERES

            if (nuevaPassword.Length < 6)
            {
                lblMensaje.Text =
                    "La contraseña debe tener al menos 6 caracteres.";

                return;
            }


            // LAS DOS DEBEN COINCIDIR

            if (nuevaPassword != confirmarPassword)
            {
                lblMensaje.Text =
                    "Las contraseñas no coinciden.";

                return;
            }


            try
            {
                UsuarioDAO dao =
                    new UsuarioDAO();


                int idUsuario =
                    dao.ObtenerUsuarioPorToken(token);


                if (idUsuario == 0)
                {
                    lblMensaje.Text =
                        "El enlace venció o ya fue utilizado.";

                    btnCambiar.Enabled =
                        false;

                    return;
                }


                // NO PUEDE SER LA CONTRASEÑA ACTUAL

                if (dao.EsPasswordActual(
                    idUsuario,
                    nuevaPassword))
                {
                    lblMensaje.Text =
                        "La contraseña ya está en uso actualmente. " +
                        "Debe elegir una contraseña diferente.";

                    return;
                }


                dao.RestablecerPassword(
                    idUsuario,
                    token,
                    nuevaPassword);


                lblMensaje.Text =
                    "Contraseña actualizada correctamente. " +
                    "Ya puede iniciar sesión con su nueva contraseña.";


                txtNuevaPassword.Text =
                    "";

                txtConfirmarPassword.Text =
                    "";

                btnCambiar.Enabled =
                    false;
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "Error: " + ex.Message;
            }
        }
    }
}
