using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Avanzada2026
{
    public partial class Ejercicio2 : System.Web.UI.Page
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
            DdCategoria.DataSource = BIZ.Categoria2.TodasLasCategorias();
            DdCategoria.DataTextField = "descripcion";
            DdCategoria.DataValueField = "id_categoria";
            DdCategoria.DataBind();

            CargarRepeater();

        }

        protected void DdCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarRepeater();
        }

        private void CargarRepeater()
        {
            RpProducto.DataSource = BIZ.Producto.ProductosPorCategoria(Convert.ToInt32(DdCategoria.SelectedValue));

            RpProducto.DataBind();
        }

    }
}
