using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Avanzada2026
{
    public partial class CrearCuenta : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void BtEnviar_Click(object sender, EventArgs e)
        {
            BIZ.Usuario.STUsuario usuario = new BIZ.Usuario.STUsuario();

            usuario.Nombre = TxNombre.Text;
            usuario.Apellido = TxApellido.Text;
            usuario.Email = TxCorreo1.Text;
            usuario.Telefono = TxCel.Text;
            usuario.password = Txpassword1.Text;

            if (FuFoto.HasFile)
            {
                byte[] imagenBytes = FuFoto.FileBytes;
                usuario.foto = imagenBytes;
                // Mostrar en el control Image como Base64
                string base64String = Convert.ToBase64String(imagenBytes);
                ImgFoto.ImageUrl = "data:image/png;base64," + base64String;
            }

                if (ValidarCampos() == "OK")
            {
                BIZ.Usuario.AgregarUsuario(usuario);
                Response.Redirect("default.aspx");
            }
            else
            {
                Response.Write("<script>alert('" + ValidarCampos() + "');</script>");
            }

        }

        private string ValidarCampos()
        {
            if (Txpassword1.Text != Txpassword2.Text)
            {
                return "Las contraseñas no coinciden.";

            }
            else if (TxCorreo1.Text != TxCorreo2.Text)
            {
                return "Los correos electrónicos no coinciden.";
            }
            else
            {
                return "OK";
            }
            // Validar otros campos según sea necesario
        }
    }
}
