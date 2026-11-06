using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eeventos
{
    public partial class EjemploCookie : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

              HttpCookie cookie = Request.Cookies["IDIOMA"];
                if (cookie != null) {
                    lbmensaje.Text = cookie.Value;
                    DdIdioma.Items.FindByText(cookie.Value).Selected = true;
                }
                Inicio();
            }
        }


        protected void DdIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            Inicio();
            CrearCookie();
        }

        private void CrearCookie()
        {
            HttpCookie cookiejemplo = new HttpCookie("IDIOMA");
            cookiejemplo.Value = DdIdioma.SelectedItem.Text;
            cookiejemplo.Expires = DateTime.Now.AddMinutes(4);

            Response.Cookies.Add(cookiejemplo);

            lbmensaje.Text = "Cookie guardada : " + cookiejemplo.Value;

        }

        private void Inicio()
        {
            switch (DdIdioma.SelectedValue.ToString()) {
                case "0":
                    lbmensaje.Text = "Español";
                    break;
                case "1":
                    lbmensaje.Text = "Ingles";
                    break;
                case "2":
                    lbmensaje.Text = "Chino";
                    break;
                default:
                    break;
            }
        }






    }
}
