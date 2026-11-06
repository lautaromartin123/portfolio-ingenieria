<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="PanelRecepcionista.aspx.cs"
    Inherits="Consultorios.PanelRecepcionista" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Recepción - Consultorios</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />

    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>

<form
    id="form1"
    runat="server"
    class="contenedor panel-recepcion">

    <div class="encabezado">
<h2 class="titulo-panel">
        Módulo Recepcionista
    </h2>

    <asp:Label
        ID="lblUsuario"
        runat="server"

        CssClass="usuario">
    </asp:Label>

    <p class="subtitulo-panel">
        Accedé a las funciones habilitadas para tu rol.
    </p>
</div>

    <!-- ========================================= -->
    <!-- AGENDA -->
    <!-- ========================================= -->

    <div class="grupo-accesos">
<asp:Panel
        ID="pnlAgenda"
        runat="server"

        CssClass="tarjeta menu">

        <h3>
            Agenda
        </h3>

        <p class="opcion">

            <asp:HyperLink
                ID="lnkAgenda"
                runat="server"
                NavigateUrl="~/AgendaProfesional.aspx"
                Text="Agenda de profesionales">
            </asp:HyperLink>

        </p>

    </asp:Panel>

    <!-- ========================================= -->
    <!-- TURNOS -->
    <!-- ========================================= -->

    <asp:Panel
        ID="pnlTurnos"
        runat="server"

        CssClass="tarjeta menu">

        <h3>
            Turnos
        </h3>

        <p class="opcion">

            <asp:HyperLink
                ID="lnkTurnos"
                runat="server"
                NavigateUrl="~/ListadoTurnos.aspx"
                Text="Gestionar turnos">
            </asp:HyperLink>

        </p>

    </asp:Panel>
    </div>

    <!-- ========================================= -->
    <!-- SIN OPCIONES -->
    <!-- ========================================= -->

    <asp:Label
        ID="lblSinOpciones"
        runat="server"
        CssClass="sin-opciones"
        Visible="false"
        Text="No tiene funciones habilitadas actualmente.">
    </asp:Label>

    <br />

    <!-- ========================================= -->
    <!-- CERRAR SESIÓN -->
    <!-- ========================================= -->

    <asp:Button
        ID="btnCerrarSesion"
        runat="server"
        Text="Cerrar sesión"
        CssClass="boton boton-secundario"
        OnClick="BtnCerrarSesion_Click" />

</form>

</body>

</html>
