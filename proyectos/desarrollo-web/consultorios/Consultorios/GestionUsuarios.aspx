<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="GestionUsuarios.aspx.cs"
    Inherits="Consultorios.GestionUsuarios" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Gestión de usuarios - Consultorios</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body class="pagina-gestion">

<form id="form1" runat="server">

    <div class="contenedor">

        <div class="encabezado">
<h1 class="titulo">
            GESTIÓN DE USUARIOS
        </h1>

        <p class="subtitulo">
            Administrá el nivel y estado de las cuentas.
        </p>
</div>

        <div class="campo">

            <asp:Label
                ID="LabelUsuario"
                runat="server"
                CssClass="campo-label"
                Text="SELECCIONAR USUARIO">
            </asp:Label>

            <asp:DropDownList
                ID="ddlUsuarios"
                runat="server"
                CssClass="select"
                AutoPostBack="true"
                OnSelectedIndexChanged="DdlUsuarios_SelectedIndexChanged">
            </asp:DropDownList>

        </div>

        <div class="datos-usuario">

            <asp:Label
                ID="lblNombre"
                runat="server"

                CssClass="dato">
            </asp:Label>

            <asp:Label
                ID="lblApellido"
                runat="server"

                CssClass="dato">
            </asp:Label>

            <asp:Label
                ID="lblEmail"
                runat="server"

                CssClass="dato">
            </asp:Label>

        </div>

        <div class="campo">

            <asp:Label
                ID="LabelNivel"
                runat="server"
                CssClass="campo-label"
                Text="NIVEL">
            </asp:Label>

            <asp:DropDownList
                ID="ddlNivel"
                runat="server"

                CssClass="select">
            </asp:DropDownList>

        </div>

        <div class="campo">

            <asp:Label
                ID="LabelEstado"
                runat="server"
                CssClass="campo-label"
                Text="ESTADO">
            </asp:Label>

            <asp:DropDownList
                ID="ddlEstado"
                runat="server"

                CssClass="select">
            </asp:DropDownList>

        </div>

        <asp:Button
            ID="btnGuardar"
            runat="server"
            CssClass="btn-principal"
            Text="GUARDAR CAMBIOS"
            OnClick="BtnGuardar_Click" />

        <asp:Label
            ID="lblMensaje"
            runat="server"

            CssClass="mensaje">
        </asp:Label>

        <div class="volver">

            <asp:HyperLink
                ID="lnkVolver"
                runat="server"
                NavigateUrl="~/PanelAdmin.aspx"
                Text="← VOLVER AL PANEL">
            </asp:HyperLink>

        </div>

    </div>

</form>

</body>

</html>
