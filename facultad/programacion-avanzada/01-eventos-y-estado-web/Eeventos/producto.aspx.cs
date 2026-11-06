using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eeventos
{
    public partial class producto : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Cargaproducto();
            }
        }

        private void Cargaproducto()
        {
            string id = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(id))
            {
                BIZ.Producto.ProductoFull producto = BIZ.Producto.ObtenerProductoById(Convert.ToInt32(id));
                TdId.Text = producto.Id.ToString();
                TxDescripcion.Text = producto.Descripcion;
                TxCodigo.Text = producto.Codigo;
                TxCosto.Text = producto.Costo.ToString();
            }
        }
    }
}
