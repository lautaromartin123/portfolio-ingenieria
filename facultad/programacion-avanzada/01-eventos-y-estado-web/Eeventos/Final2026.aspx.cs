using BIZ;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    namespace Eeventos
    {
        public partial class Final2026 : System.Web.UI.Page
        {

            CategoriaNegocio categoriaNegocio = new CategoriaNegocio();
            ProductoNegocio productoNegocio = new ProductoNegocio();


            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack)
                {
                    CargarCategorias();
                    CargarSubCategoriasInicial();
                }

            }


            private void CargarCategorias()
            {
                DdCategoria.DataSource = categoriaNegocio.ListarCategoriasPrincipales();

                DdCategoria.DataTextField = "Descripcion";
                DdCategoria.DataValueField = "IdCategoria";

                DdCategoria.DataBind();

                DdCategoria.Items.Insert(0, new ListItem("SIN CATEGORIA", "0"));

            }

            private void CargarSubCategoriasInicial()
            {
                DdSubCategoria.Items.Clear();
                DdSubCategoria.Items.Insert(0, new ListItem("SIN SUBCATEGORIA", "0"));

            }

            private void CargarSubCategorias(int idCategoria)
            {

                DdSubCategoria.DataSource = categoriaNegocio.ListarSubcategorias(idCategoria);
                DdSubCategoria.DataTextField = "Descripcion";
                DdSubCategoria.DataValueField = "IdCategoria";

                DdSubCategoria.DataBind();
                DdSubCategoria.Items.Insert(0, new ListItem("SIN SUBCATEGORIA", "0"));


            }

            protected void DdSubCategoria_SelectedIndexChanged(object sender, EventArgs e)
            {
                CargarProductos();
            }

            protected void DdCategoria_SelectedIndexChanged(object sender, EventArgs e)
            {
                int idCategoria;
                if (int.TryParse(DdCategoria.SelectedValue, out idCategoria) && idCategoria != 0)
                    CargarSubCategorias(idCategoria);
                else
                    CargarSubCategoriasInicial();
                CargarProductos();
            }

            private void CargarProductos()
            {
                if (DdCategoria.SelectedValue == "0")
                {
                    LimpiarRepeater();
                    return;
                }
                int idCategoriaGeneral = int.Parse(DdCategoria.SelectedValue);

                if(DdSubCategoria.SelectedValue == "0")
                {
                    RepProductos.DataSource = productoNegocio.Listar(idCategoriaGeneral);
                }else
                {
                    int idSubCategoria = int.Parse(DdSubCategoria.SelectedValue);
                    RepProductos.DataSource = productoNegocio.Listar(idCategoriaGeneral, idSubCategoria);

                }
            RepProductos.DataBind();



            }

            private void LimpiarRepeater()
            {
                RepProductos.DataSource = null;
                RepProductos.DataBind();
            }










        }
    }
