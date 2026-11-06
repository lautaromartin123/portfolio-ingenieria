<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="PanelAdmin.aspx.cs"
    Inherits="Consultorios.PanelAdmin" ResponseEncoding="utf-8" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <title>Administracion</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />

</head>

<body>

<form
    id="form1"
    runat="server"
    class="contenedor">

    <div class="encabezado">
    <h2>
        Modulo Administrador
    </h2>

    <asp:Label
        ID="lblUsuario"
        runat="server"

        CssClass="usuario">
    </asp:Label>
    <p class="ayuda">Administra los profesionales, los turnos y las solicitudes pendientes.</p>
    </div>

    <!-- ========================================= -->
    <!-- PROFESIONALES / AGENDA / TURNOS -->
    <!-- ========================================= -->

    <div class="grupo-accesos">
<asp:Panel
        ID="pnlProfesionalesTurnos"
        runat="server"

        CssClass="tarjeta menu">

        <h3>
            Profesionales y turnos
        </h3>

        <ul>

            <li>

                <asp:HyperLink
                    ID="lnkProfesionales"
                    runat="server"
                    NavigateUrl="~/ListadoProfesionales.aspx"
                    Text="Profesionales">
                </asp:HyperLink>

            </li>

            <li>

                <asp:HyperLink
                    ID="lnkAgenda"
                    runat="server"
                    NavigateUrl="~/AgendaProfesional.aspx"
                    Text="Agenda de profesionales">
                </asp:HyperLink>

            </li>

            <li>

                <asp:HyperLink
                    ID="lnkTurnos"
                    runat="server"
                    NavigateUrl="~/ListadoTurnos.aspx"
                    Text="Turnos">
                </asp:HyperLink>

            </li>

        </ul>

    </asp:Panel>

    <!-- ========================================= -->
    <!-- GESTION -->
    <!-- ========================================= -->

    <div class="tarjeta menu">
        <h3>Revision de horarios</h3>
        <p><asp:HyperLink ID="lnkSolicitudesAgenda" runat="server" NavigateUrl="~/SolicitudesAgenda.aspx" Text="Solicitudes de horarios de medicos" /></p>
    </div>

    <asp:Panel
        ID="pnlGestion"
        runat="server"

        CssClass="tarjeta menu">

        <h3>
            Gestion
        </h3>

        <p>

            <asp:HyperLink
                ID="lnkGestionUsuarios"
                runat="server"
                NavigateUrl="~/GestionUsuarios.aspx"
                Text="Gestionar usuarios">
            </asp:HyperLink>

        </p>

        <p>

            <asp:HyperLink
                ID="lnkGestionPermisos"
                runat="server"
                NavigateUrl="~/GestionPermisos.aspx"
                Text="Gestionar permisos">
            </asp:HyperLink>

        </p>

        <p>

            <asp:HyperLink
                ID="lnkSolicitudesModificacion"
                runat="server"
                NavigateUrl="~/SolicitudesModificacion.aspx"
                Text="Solicitudes de modificacion">
            </asp:HyperLink>

        </p>

    </asp:Panel>
    </div>

    <!-- ========================================= -->
    <!-- CERRAR SESION -->
    <!-- ========================================= -->

    <asp:Button
        ID="btnCerrarSesion"
        runat="server"
        Text="Cerrar sesion"
        CssClass="boton boton-secundario"
        OnClick="btnCerrarSesion_Click" />

</form>

</body>

</html>
