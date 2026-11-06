using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eeventos
{
    public partial class db : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargaInicial();
            }
        }

        private void CargaInicial()
        {
            DDDinamico.DataSource = BIZ.Estado.ConsultarUnEstado("9");
            DDDinamico.DataTextField = "descripcion";
            DDDinamico.DataValueField = "id_estado";
            DDDinamico.DataBind();
        }

        protected void DdEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Write("Estado: " + DdEstado.SelectedValue);
        }
    }
}
