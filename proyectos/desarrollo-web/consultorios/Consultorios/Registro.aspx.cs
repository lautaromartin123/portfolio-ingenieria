using System;

namespace Consultorios
{
    public partial class Registro :
        System.Web.UI.Page
    {
        protected void Page_Load(
            object sender,
            EventArgs e)
        {

        }


        protected void BtnRegistrar_Click(
            object sender,
            EventArgs e)
        {
            string nombre =
                txtNombre.Text.Trim();

            string apellido =
                txtApellido.Text.Trim();

            string email =
                txtEmail.Text
                    .Trim()
                    .ToLowerInvariant();

            string password =
                txtPassword.Text;

            string confirmar =
                txtConfirmarPassword.Text;


            // ==========================================
            // CAMPOS OBLIGATORIOS
            // ==========================================

            if (nombre == "" ||
                apellido == "" ||
                email == "" ||
                password == "" ||
                confirmar == "")
            {
                lblMensaje.Text =
                    "Debe completar todos los campos.";

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


            // ==========================================
            // CONTRASEÑA
            // ==========================================

            if (password.Length < 6)
            {
                lblMensaje.Text =
                    "La contraseña debe tener al menos 6 caracteres.";

                return;
            }


            if (password != confirmar)
            {
                lblMensaje.Text =
                    "Las contraseñas no coinciden.";

                return;
            }


            try
            {
                UsuarioDAO dao =
                    new UsuarioDAO();


                // ======================================
                // EMAIL NO PUEDE REPETIRSE
                // ======================================

                if (dao.ExisteEmail(email))
                {
                    lblMensaje.Text =
                        "Ya existe una cuenta asociada a ese Gmail.";

                    return;
                }


                // ======================================
                // REGISTRAR PACIENTE
                // ======================================

                dao.RegistrarPaciente(
                    nombre,
                    apellido,
                    email,
                    password);


                lblMensaje.Text =
                    "Cuenta creada correctamente. " +
                    "Ya puede iniciar sesión con su Gmail.";


                // ======================================
                // LIMPIAR CAMPOS
                // ======================================

                txtNombre.Text = "";
                txtApellido.Text = "";
                txtEmail.Text = "";
                txtPassword.Text = "";
                txtConfirmarPassword.Text = "";
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "Error: " + ex.Message;
            }
        }
    }
}
