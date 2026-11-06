<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="GestionPermisos.aspx.cs"
    Inherits="Consultorios.GestionPermisos" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Gestión de permisos - Consultorios</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body class="pagina-gestion">

<form id="form1" runat="server">

    <div class="contenedor">

        <div class="cabecera">

            <h1 class="titulo">
                GESTIÓN DE PERMISOS
            </h1>

            <p class="subtitulo">
                Configurá el acceso y las acciones permitidas para cada rol.
            </p>

        </div>

        <div class="tarjeta">

            <asp:Label
                ID="lblNivel"
                runat="server"
                CssClass="campo-label"
                Text="ROL / NIVEL">
            </asp:Label>

            <asp:DropDownList
                ID="ddlNivel"
                runat="server"
                CssClass="select"
                AutoPostBack="true"
                OnSelectedIndexChanged="DdlNivel_SelectedIndexChanged">
            </asp:DropDownList>

            <div class="seccion">

                <h2 class="seccion-titulo">
                    ACCESO A PÁGINAS
                </h2>

                <p class="seccion-texto">
                    Define a qué pantallas del sistema puede ingresar este rol.
                </p>

                <asp:CheckBoxList
                    ID="cblPaginas"
                    runat="server"
                    CssClass="check-list"
                    RepeatColumns="2"
                    RepeatDirection="Vertical">
                </asp:CheckBoxList>

            </div>

            <div class="seccion">

                <h2 class="seccion-titulo">
                    PERMISOS DE ACCIÓN
                </h2>

                <p class="seccion-texto">
                    Define qué operaciones puede realizar este rol dentro del sistema.
                </p>

                <asp:CheckBoxList
                    ID="cblPermisos"
                    runat="server"
                    CssClass="check-list"
                    RepeatColumns="2"
                    RepeatDirection="Vertical">
                </asp:CheckBoxList>

            </div>

            <asp:Button
                ID="btnGuardar"
                runat="server"
                CssClass="btn-principal"
                Text="GUARDAR PERMISOS"
                OnClick="BtnGuardar_Click" />

            <asp:Label
                ID="lblMensaje"
                runat="server"

                CssClass="mensaje">
            </asp:Label>

        </div>

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
