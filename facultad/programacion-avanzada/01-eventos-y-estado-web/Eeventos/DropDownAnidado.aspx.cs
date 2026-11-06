using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eeventos
{
    public partial class DropDownAnidado : System.Web.UI.Page
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
            RpEjemplo.DataSource = BIZ.Categoria.CategoriaPadre();
            RpEjemplo.DataBind();

            DdPadrea.DataSource = BIZ.Categoria.CategoriaPadre();
            DdPadrea.DataTextField = "descripcion";
            DdPadrea.DataValueField = "id_categoria";
            DdPadrea.DataBind();

            CargarHijos();

        }

        protected void DdPadrea_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarHijos();
        }

        private void CargarHijos()
        {
            DdHijo.DataSource = BIZ.Categoria.CategoriaHijo(DdPadrea.SelectedValue.ToString());
            DdHijo.DataTextField = "descripcion";
            DdHijo.DataValueField = "id_categoria";
            DdHijo.DataBind();
        }
    }
}
