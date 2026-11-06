using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eeventos
{
    public partial class Repeater : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargaInical();
            }
        }

        private void CargaInical()
        {
            RpProducto.DataSource = BIZ.Producto.ObtenerProducto();
            RpProducto.DataBind();
            Ddcategoria.DataSource = BIZ.Categoria.CategoriaPadre().Tables[0];
            Ddcategoria.DataTextField = "Descripcion";
            Ddcategoria.DataValueField = "Id_Categoria";
            Ddcategoria.DataBind();
        }

        protected void RpProducto_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
             HiddenField HfId = (HiddenField)e.Item.FindControl("HfId");
             CargarProductoUnico(HfId.Value.ToString());
            //Response.Write("Id del producto: " + HfId.Value.ToString());
        }

        private void CargarProductoUnico(string id)
        {
          Response.Redirect("Producto.aspx?id=" + id);
        }

        protected void txbuscar_TextChanged(object sender, EventArgs e)
        {
            RpProducto.DataSource = BIZ.Producto.ObtenerProductoPorDescripcion(txbuscar.Text);
            RpProducto.DataBind();
        }

        protected void Ddcategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            RpProducto.DataSource = BIZ.Producto.ObtenerProductoPorIdcategoria(Ddcategoria.SelectedValue);
            RpProducto.DataBind();
        }
    }
}
