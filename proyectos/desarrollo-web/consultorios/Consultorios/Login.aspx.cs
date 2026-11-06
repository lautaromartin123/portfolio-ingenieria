using System;

namespace Consultorios
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(
            object sender,
            EventArgs e)
        {

        }


        protected void BtnIngresar_Click(
            object sender,
            EventArgs e)
        {
            string email =
                txtEmail.Text.Trim();

            string password =
                txtPassword.Text;


            if (email == "" ||
                password == "")
            {
                lblMensaje.Text =
                    "Debe ingresar Gmail y contraseña.";

                return;
            }


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


                Usuario usuario =
                    dao.Login(
                        email,
                        password);


                if (usuario == null)
                {
                    lblMensaje.Text =
                        "Gmail o contraseña incorrectos.";

                    return;
                }


                // ==========================================
                // CREAR SESIÓN
                // ==========================================

                Session["IdUsuario"] =
                    usuario.IdUsuario;

                Session["Nombre"] =
                    usuario.Nombre;

                Session["Apellido"] =
                    usuario.Apellido;

                Session["Email"] =
                    usuario.Email;

                Session["IdNivel"] =
                    usuario.IdNivel;

                Session["Nivel"] =
                    usuario.Nivel;


                // ==========================================
                // REDIRECCIÓN SEGÚN NIVEL
                // ==========================================

                switch (usuario.Nivel)
                {
                    case "ADMIN":

                        Response.Redirect(
                            "PanelAdmin.aspx");

                        break;


                    case "RECEPCIONISTA":

                        Response.Redirect(
                            "PanelRecepcionista.aspx");

                        break;


                    case "MEDICO":

                        Response.Redirect(
                            "PanelMedico.aspx");

                        break;


                    case "PACIENTE":

                        Response.Redirect(
                            "PanelPaciente.aspx");

                        break;


                    default:

                        Session.Clear();

                        lblMensaje.Text =
                            "El usuario no tiene un nivel válido.";

                        break;
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "Error: " + ex.Message;
            }
        }
    }
}
