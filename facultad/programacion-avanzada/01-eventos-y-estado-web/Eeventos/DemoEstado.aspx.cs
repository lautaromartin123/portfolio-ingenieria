using System;
using System.Web;

namespace Eeventos
{
    public partial class DemoEstado : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                lblPostBack.Text = "La página volvió al servidor mediante PostBack.";
            }
            else
            {
                lblPostBack.Text = "Primera carga de la página.";

                ViewState["clicks"] = 0;
            }

            if (Session["visitas"] == null)
            {
                Session["visitas"] = 1;
            }
            else
            {
                Session["visitas"] = (int)Session["visitas"] + 1;
            }

            lblSession.Text = "Session - visitas/interacciones: " + Session["visitas"];

            if (Request.Cookies["usuario"] != null)
            {
                lblCookie.Text = "Cookie - usuario guardado: " + Request.Cookies["usuario"].Value;
            }
            else
            {
                lblCookie.Text = "Cookie - todavía no hay usuario guardado.";
            }

            lblViewState.Text = "ViewState - clicks en esta página: " + ViewState["clicks"];
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            HttpCookie cookieUsuario = new HttpCookie("usuario");
            cookieUsuario.Value = txtNombre.Text;
            cookieUsuario.Expires = DateTime.Now.AddDays(7);

            Response.Cookies.Add(cookieUsuario);

            ViewState["clicks"] = (int)ViewState["clicks"] + 1;

            lblCookie.Text = "Cookie - usuario guardado: " + txtNombre.Text;
            lblViewState.Text = "ViewState - clicks en esta página: " + ViewState["clicks"];
        }



    }
}
