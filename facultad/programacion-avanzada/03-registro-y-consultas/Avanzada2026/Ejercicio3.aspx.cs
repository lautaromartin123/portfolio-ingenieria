using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Avanzada2026
{
    public partial class Ejercicio3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) { CargarInicial(); }
        }

    private void CargarInicial()
        {
            DdEstado.DataSource = BIZ.Estado.TodosLosEstados();
            DdEstado.DataTextField = "descripcion";
            DdEstado.DataValueField = "id_estado";
            DdEstado.DataBind();

            CargarRepeater();
        }

    protected void DdEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarRepeater();
        }

    private void CargarRepeater()
        {
            RpUsuario.DataSource = BIZ.UsuarioConsulta.UsuariosPorEstado(Convert.ToInt32(DdEstado.SelectedValue));
            RpUsuario.DataBind();
        }

    }
}
