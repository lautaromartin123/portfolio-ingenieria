<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="PanelMedico.aspx.cs"
    Inherits="Consultorios.PanelMedico" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <meta
        name="viewport"
        content="width=device-width, initial-scale=1" />

    <title>Panel Médico</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />

</head>

<body class="pagina-medico">

<form
    id="form1"
    runat="server"
    class="medico-shell">

    <header class="medico-header">

        <span class="medico-badge">
            Área profesional
        </span>

        <h1>
            Panel Médico
        </h1>

        <p class="medico-subtitulo">
            Gestión de agenda, turnos y asistencia operativa.
        </p>

        <div class="medico-usuario">

            <asp:Label
                ID="lblUsuario"
                runat="server"

                CssClass="usuario">
            </asp:Label>

        </div>

    </header>

    <asp:Panel
        ID="pnlFunciones"
        runat="server"

        CssClass="medico-panel">

        <div class="medico-panel-header">

            <h2>
                Funciones disponibles
            </h2>

            <p>
                Los accesos mostrados corresponden a los
                permisos habilitados para tu cuenta.
            </p>

        </div>

        <div class="medico-accesos">

            <asp:HyperLink
                ID="lnkAgenda"
                runat="server"
                NavigateUrl="~/AgendaProfesional.aspx"
                CssClass="medico-link medico-link-agenda"
                Text="Agenda profesional">
            </asp:HyperLink>

            <asp:HyperLink
                ID="lnkTurnos"
                runat="server"
                NavigateUrl="~/ListadoTurnos.aspx"
                CssClass="medico-link medico-link-turnos"
                Text="Gestión de turnos">
            </asp:HyperLink>

            <asp:Button
                ID="btnIA"
                runat="server"
                Text="IA y reportes"
                CssClass="medico-link medico-link-ia"
                CausesValidation="false"
                OnClick="BtnIA_Click" />

        </div>

        <asp:Label
            ID="lblSinOpciones"
            runat="server"
            Visible="false"
            CssClass="medico-vacio"
            Text="No tenés funciones habilitadas actualmente.">
        </asp:Label>

        <div class="medico-footer">

            <span class="medico-footer-texto">
                Consultorios · Sistema de gestión médica
            </span>

            <asp:Button
                ID="btnCerrarSesion"
                runat="server"
                Text="Cerrar sesión"
                CssClass="boton medico-boton-salir"
                OnClick="BtnCerrarSesion_Click" />

        </div>

    </asp:Panel>

</form>

</body>

</html>
