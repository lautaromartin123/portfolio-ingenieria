using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Avanzada2026
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ltMensaje.Visible = false;
            }
        }

        protected void BtLogin_Click(object sender, EventArgs e)
        {
            BIZ.Usuario usuariologin = new BIZ.Usuario();
            string resultado = usuariologin.LoginUsuario(Txusuario.Text,TxPassword.Text);
            if (resultado == "OK")
            {
                    // Redirigir a la página principal o realizar alguna acción
                    Response.Redirect("PaginaPrincipal.aspx");
            }
            else
            {
                ltMensaje.Text = "<h3>Usuario o contraseña incorrectos</h3>";
                ltMensaje.Visible = true;
                // Mostrar mensaje de error
            }
        }
    }
}
