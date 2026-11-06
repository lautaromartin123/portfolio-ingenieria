using System;
using BIZ;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Avanzada2026
{
    public partial class ConsultaProductos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarInicial();
            }
        }


        private void CargarInicial()
        {
            DdEstado.DataSource = EstadoNegocio.ListarEstados();
            DdEstado.DataTextField = "nombre";
            DdEstado.DataValueField = "IdEstado";

            DdEstado.DataBind();

            CargarProductos();

        }

        protected void DdEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarProductos();
        }

        private void CargarProductos()
        {
            RepProductos.DataSource = ProductoNegocio.ListarProductosPorEstado(Convert.ToInt32(DdEstado.SelectedValue));
            RepProductos.DataBind();

        }














    }
}
