using System;
using System.Web;
using System.Web.UI;

namespace Consultorios
{
    public static class Seguridad
    {
        // =====================================================
        // COMPROBAR ROL / NIVEL
        // =====================================================

        public static bool RequerirRol(
            Page pagina,
            string nivelPermitido)
        {
            if (HttpContext.Current.Session["IdUsuario"] == null ||
                HttpContext.Current.Session["Nivel"] == null)
            {
                pagina.Response.Redirect(
                    pagina.ResolveUrl("~/Login.aspx"));

                return false;
            }

            string nivelUsuario =
                HttpContext.Current
                    .Session["Nivel"]
                    .ToString();

            if (!nivelUsuario.Equals(
                nivelPermitido,
                StringComparison.OrdinalIgnoreCase))
            {
                pagina.Response.Redirect(
                    pagina.ResolveUrl("~/Login.aspx"));

                return false;
            }

            return true;
        }


        // =====================================================
        // CERRAR SESIÓN
        // =====================================================

        public static void CerrarSesion(
            Page pagina)
        {
            HttpContext.Current
                .Session.Clear();

            HttpContext.Current
                .Session.Abandon();

            pagina.Response.Redirect(
                pagina.ResolveUrl("~/Login.aspx"));
        }


        // =====================================================
        // CONTROL DE ACCESO A PÁGINA
        // =====================================================

        public static bool RequerirAcceso(
            Page pagina)
        {
            if (HttpContext.Current.Session["IdUsuario"] == null ||
                HttpContext.Current.Session["IdNivel"] == null)
            {
                pagina.Response.Redirect(
                    pagina.ResolveUrl("~/Login.aspx"));

                return false;
            }

            int idNivel =
                Convert.ToInt32(
                    HttpContext.Current
                        .Session["IdNivel"]);

            string paginaActual =
                System.IO.Path.GetFileName(
                    pagina.Request
                        .AppRelativeCurrentExecutionFilePath);

            UsuarioDAO dao =
                new UsuarioDAO();

            // Si la página todavía no existe,
            // se registra en dbo.URL.
            dao.RegistrarUrlSiNoExiste(
                paginaActual);

            bool tieneAcceso =
                dao.TieneAcceso(
                    idNivel,
                    paginaActual);

            if (!tieneAcceso)
            {
                pagina.Response.Redirect(
                    pagina.ResolveUrl("~/Login.aspx"));

                return false;
            }

            return true;
        }


        // =====================================================
        // CONSULTAR PERMISO DE ACCIÓN
        // =====================================================

        public static bool TienePermiso(
            string codigoPermiso)
        {
            if (HttpContext.Current.Session["IdUsuario"] == null ||
                HttpContext.Current.Session["IdNivel"] == null)
            {
                return false;
            }

            int idNivel =
                Convert.ToInt32(
                    HttpContext.Current
                        .Session["IdNivel"]);

            UsuarioDAO dao =
                new UsuarioDAO();

            return dao.TienePermiso(
                idNivel,
                codigoPermiso);
        }


        // =====================================================
        // EXIGIR PERMISO DE ACCIÓN
        // =====================================================

        public static bool RequerirPermiso(
            Page pagina,
            string codigoPermiso)
        {
            if (HttpContext.Current.Session["IdUsuario"] == null ||
                HttpContext.Current.Session["IdNivel"] == null)
            {
                pagina.Response.Redirect(
                    pagina.ResolveUrl("~/Login.aspx"));

                return false;
            }

            if (!TienePermiso(
                codigoPermiso))
            {
                pagina.Response.Redirect(
                    pagina.ResolveUrl("~/Login.aspx"));

                return false;
            }

            return true;
        }
    }
}
