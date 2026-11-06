using System;

namespace Consultorios
{
    public partial class RecuperarPassword :
        System.Web.UI.Page
    {
        protected void Page_Load(
            object sender,
            EventArgs e)
        {

        }


        protected void BtnRecuperar_Click(
            object sender,
            EventArgs e)
        {
            string email =
                txtEmail.Text
                    .Trim()
                    .ToLowerInvariant();


            // ==========================================
            // CAMPO OBLIGATORIO
            // ==========================================

            if (email == "")
            {
                lblMensaje.Text =
                    "Ingrese su Gmail.";

                return;
            }


            // ==========================================
            // VALIDAR GMAIL
            // ==========================================

            if (!email.EndsWith(
                "@gmail.com",
                StringComparison.OrdinalIgnoreCase))
            {
                lblMensaje.Text =
                    "Debe ingresar una dirección de Gmail válida.";

                return;
            }


            try
            {
                UsuarioDAO dao =
                    new UsuarioDAO();


                // ======================================
                // COMPROBAR SI EL EMAIL EXISTE
                // ======================================

                if (!dao.ExisteEmail(email))
                {
                    lblMensaje.Text =
                        "No existe una cuenta asociada a ese correo.";

                    return;
                }


                // ======================================
                // CREAR TOKEN
                // ======================================

                /*
                 * CrearRecuperacionPassword ahora busca
                 * al usuario mediante Email.
                 *
                 * Si existe pero está inactivo,
                 * devuelve null.
                 */

                string token =
                    dao.CrearRecuperacionPassword(
                        email);


                if (string.IsNullOrEmpty(token))
                {
                    lblMensaje.Text =
                        "La cuenta asociada a ese correo está inactiva.";

                    return;
                }


                // ======================================
                // CREAR URL DE RECUPERACIÓN
                // ======================================

                string urlRecuperacion =
                    Request.Url.GetLeftPart(
                        UriPartial.Authority)
                    +
                    ResolveUrl(
                        "~/ResetPassword.aspx?token=" +
                        Server.UrlEncode(token));


                // ======================================
                // ENVIAR GMAIL
                // ======================================

                EmailService
                    .EnviarRecuperacionPassword(
                        email,
                        email,
                        urlRecuperacion);


                lblMensaje.Text =
                    "Se envió un correo electrónico a " +
                    email +
                    " con las instrucciones para " +
                    "restablecer la contraseña.";


                txtEmail.Text = "";
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "No se pudo enviar el correo. " +
                    ex.Message;
            }
        }
    }
}
