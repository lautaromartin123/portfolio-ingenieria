using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

namespace Consultorios
{
    public partial class GestionPermisos :
        System.Web.UI.Page
    {
        // =====================================================
        // PAGE LOAD
        // =====================================================

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!Seguridad.RequerirAcceso(this))
            {
                return;
            }


            if (!Seguridad.RequerirPermiso(
                this,
                "PERMISOS_GESTIONAR"))
            {
                return;
            }


            if (!IsPostBack)
            {
                CargarNiveles();


                if (ddlNivel.Items.Count > 0)
                {
                    CargarConfiguracionNivel();
                }
            }
        }


        // =====================================================
        // SABER SI EL NIVEL SELECCIONADO ES ADMIN
        // =====================================================

        private bool EsNivelAdmin()
        {
            if (ddlNivel.SelectedItem == null)
            {
                return false;
            }


            return ddlNivel.SelectedItem.Text.Equals(
                "ADMIN",
                StringComparison.OrdinalIgnoreCase);
        }


        // =====================================================
        // CARGAR NIVELES
        // =====================================================

        private void CargarNiveles()
        {
            UsuarioDAO dao =
                new UsuarioDAO();


            ddlNivel.DataSource =
                dao.ObtenerNiveles();


            ddlNivel.DataTextField =
                "Descripcion";


            ddlNivel.DataValueField =
                "IdNivel";


            ddlNivel.DataBind();
        }


        // =====================================================
        // CARGAR CONFIGURACIÓN
        // =====================================================

        private void CargarConfiguracionNivel()
        {
            int idNivel =
                Convert.ToInt32(
                    ddlNivel.SelectedValue);


            CargarPaginas(
                idNivel);


            CargarPermisos(
                idNivel);
        }


        // =====================================================
        // CARGAR ACCESO A PÁGINAS
        // =====================================================

        private void CargarPaginas(
            int idNivel)
        {
            UsuarioDAO dao =
                new UsuarioDAO();


            DataTable tabla =
                dao.ObtenerAccesosNivel(
                    idNivel);


            cblPaginas.DataSource =
                tabla;


            cblPaginas.DataTextField =
                "Descripcion";


            cblPaginas.DataValueField =
                "IdUrl";


            cblPaginas.DataBind();


            HashSet<int> seleccionados =
                new HashSet<int>();


            foreach (DataRow fila
                in tabla.Rows)
            {
                bool tieneAcceso =
                    Convert.ToBoolean(
                        fila["TieneAcceso"]);


                if (tieneAcceso)
                {
                    seleccionados.Add(
                        Convert.ToInt32(
                            fila["IdUrl"]));
                }
            }


            foreach (ListItem item
                in cblPaginas.Items)
            {
                int idUrl =
                    Convert.ToInt32(
                        item.Value);


                item.Selected =
                    seleccionados.Contains(
                        idUrl);


                // =============================================
                // PROTECCIÓN DEL NIVEL ADMIN
                // =============================================

                if (EsNivelAdmin())
                {
                    if (item.Text.Equals(
                            "PanelAdmin.aspx",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        item.Text.Equals(
                            "GestionPermisos.aspx",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        // ADMIN siempre debe conservar
                        // estas páginas.

                        item.Selected =
                            true;


                        // Impide desmarcarlas desde
                        // la interfaz.

                        item.Enabled =
                            false;
                    }
                }
            }
        }


        // =====================================================
        // CARGAR PERMISOS DE ACCIÓN
        // =====================================================

        private void CargarPermisos(
            int idNivel)
        {
            UsuarioDAO dao =
                new UsuarioDAO();


            DataTable tabla =
                dao.ObtenerPermisosNivel(
                    idNivel);


            cblPermisos.DataSource =
                tabla;


            cblPermisos.DataTextField =
                "Texto";


            cblPermisos.DataValueField =
                "IdPermiso";


            cblPermisos.DataBind();


            HashSet<int> seleccionados =
                new HashSet<int>();


            foreach (DataRow fila
                in tabla.Rows)
            {
                bool tienePermiso =
                    Convert.ToBoolean(
                        fila["TienePermiso"]);


                if (tienePermiso)
                {
                    seleccionados.Add(
                        Convert.ToInt32(
                            fila["IdPermiso"]));
                }
            }


            foreach (ListItem item
                in cblPermisos.Items)
            {
                int idPermiso =
                    Convert.ToInt32(
                        item.Value);


                item.Selected =
                    seleccionados.Contains(
                        idPermiso);
            }


            // =============================================
            // PROTEGER PERMISOS_GESTIONAR PARA ADMIN
            // =============================================

            if (EsNivelAdmin())
            {
                foreach (DataRow fila
                    in tabla.Rows)
                {
                    string codigo =
                        fila["Codigo"].ToString();


                    if (codigo.Equals(
                        "PERMISOS_GESTIONAR",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        string idPermiso =
                            fila["IdPermiso"]
                                .ToString();


                        ListItem item =
                            cblPermisos.Items
                                .FindByValue(
                                    idPermiso);


                        if (item != null)
                        {
                            item.Selected =
                                true;


                            item.Enabled =
                                false;
                        }


                        break;
                    }
                }
            }
        }


        // =====================================================
        // CAMBIO DE NIVEL
        // =====================================================

        protected void DdlNivel_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarConfiguracionNivel();


            lblMensaje.Text =
                "";
        }


        // =====================================================
        // VALIDAR QUE ADMIN NO PIERDA ACCESO CRÍTICO
        // =====================================================

        private bool ValidarProteccionAdmin()
        {
            if (!EsNivelAdmin())
            {
                return true;
            }


            bool tienePanelAdmin =
                false;


            bool tieneGestionPermisos =
                false;


            foreach (ListItem item
                in cblPaginas.Items)
            {
                if (item.Text.Equals(
                    "PanelAdmin.aspx",
                    StringComparison.OrdinalIgnoreCase))
                {
                    tienePanelAdmin =
                        item.Selected;
                }


                if (item.Text.Equals(
                    "GestionPermisos.aspx",
                    StringComparison.OrdinalIgnoreCase))
                {
                    tieneGestionPermisos =
                        item.Selected;
                }
            }


            if (!tienePanelAdmin ||
                !tieneGestionPermisos)
            {
                lblMensaje.Text =
                    "El nivel ADMIN debe conservar acceso al panel "
                    +
                    "y a Gestión de Permisos.";

                return false;
            }


            UsuarioDAO dao =
                new UsuarioDAO();


            int idNivel =
                Convert.ToInt32(
                    ddlNivel.SelectedValue);


            DataTable tabla =
                dao.ObtenerPermisosNivel(
                    idNivel);


            foreach (DataRow fila
                in tabla.Rows)
            {
                if (fila["Codigo"]
                    .ToString()
                    .Equals(
                        "PERMISOS_GESTIONAR",
                        StringComparison.OrdinalIgnoreCase))
                {
                    string idPermiso =
                        fila["IdPermiso"]
                            .ToString();


                    ListItem item =
                        cblPermisos.Items
                            .FindByValue(
                                idPermiso);


                    if (item == null ||
                        !item.Selected)
                    {
                        lblMensaje.Text =
                            "El nivel ADMIN debe conservar "
                            +
                            "el permiso PERMISOS_GESTIONAR.";

                        return false;
                    }


                    break;
                }
            }


            return true;
        }


        // =====================================================
        // GUARDAR
        // =====================================================

        protected void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            // =============================================
            // SEGURIDAD DEL USUARIO ACTUAL
            // =============================================

            if (!Seguridad.TienePermiso(
                "PERMISOS_GESTIONAR"))
            {
                lblMensaje.Text =
                    "No tiene permiso para gestionar permisos.";

                return;
            }


            // =============================================
            // EVITAR BLOQUEAR EL NIVEL ADMIN
            // =============================================

            if (!ValidarProteccionAdmin())
            {
                return;
            }


            try
            {
                int idNivel =
                    Convert.ToInt32(
                        ddlNivel.SelectedValue);


                UsuarioDAO dao =
                    new UsuarioDAO();


                // =========================================
                // PÁGINAS
                // =========================================

                foreach (ListItem item
                    in cblPaginas.Items)
                {
                    int idUrl =
                        Convert.ToInt32(
                            item.Value);


                    bool permitir =
                        item.Selected;


                    // ADMIN siempre conserva estas páginas,
                    // incluso aunque se intentara manipular
                    // manualmente el formulario.

                    if (EsNivelAdmin())
                    {
                        if (item.Text.Equals(
                                "PanelAdmin.aspx",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            item.Text.Equals(
                                "GestionPermisos.aspx",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            permitir =
                                true;
                        }
                    }


                    dao.GuardarPermiso(
                        idNivel,
                        idUrl,
                        permitir);
                }


                // =========================================
                // PERMISOS DE ACCIÓN
                // =========================================

                DataTable tablaPermisos =
                    dao.ObtenerPermisosNivel(
                        idNivel);


                foreach (ListItem item
                    in cblPermisos.Items)
                {
                    int idPermiso =
                        Convert.ToInt32(
                            item.Value);


                    bool permitir =
                        item.Selected;


                    // ADMIN siempre conserva
                    // PERMISOS_GESTIONAR.

                    if (EsNivelAdmin())
                    {
                        foreach (DataRow fila
                            in tablaPermisos.Rows)
                        {
                            if (Convert.ToInt32(
                                    fila["IdPermiso"])
                                ==
                                idPermiso)
                            {
                                if (fila["Codigo"]
                                    .ToString()
                                    .Equals(
                                        "PERMISOS_GESTIONAR",
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    permitir =
                                        true;
                                }


                                break;
                            }
                        }
                    }


                    dao.GuardarPermisoNivel(
                        idNivel,
                        idPermiso,
                        permitir);
                }


                lblMensaje.Text =
                    "Permisos actualizados correctamente.";


                CargarConfiguracionNivel();
            }
            catch (Exception ex)
            {
                lblMensaje.Text =
                    "Error: "
                    +
                    ex.Message;
            }
        }
    }
}
