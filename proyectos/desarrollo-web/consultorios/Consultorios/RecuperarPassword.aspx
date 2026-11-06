<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="RecuperarPassword.aspx.cs"
    Inherits="Consultorios.RecuperarPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Recuperar contraseña - Consultorios</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body class="pagina-acceso">

<form id="form1" runat="server">

    <div class="auth-card">

        <div class="logo">
            <div class="logo-inner"></div>
        </div>

        <h1 class="titulo">
            RECUPERAR CONTRASEÑA
        </h1>

        <p class="subtitulo">
            Ingresá el Gmail asociado a tu cuenta.
            Te enviaremos un enlace para crear una nueva contraseña.
        </p>

        <div class="campo">

            <label class="campo-label">
                GMAIL
            </label>

            <asp:TextBox
                ID="txtEmail"
                runat="server"
                CssClass="textbox"
                TextMode="Email"
                placeholder="ejemplo@gmail.com">
            </asp:TextBox>

        </div>

        <asp:Button
            ID="btnRecuperar"
            runat="server"
            CssClass="btn-principal"
            Text="ENVIAR ENLACE"
            OnClick="BtnRecuperar_Click" />

        <asp:Label
            ID="lblMensaje"
            runat="server"

            CssClass="mensaje">
        </asp:Label>

        <div class="volver">

            <asp:HyperLink
                ID="lnkLogin"
                runat="server"
                NavigateUrl="~/Login.aspx"
                Text="← VOLVER AL INICIO DE SESIÓN">
            </asp:HyperLink>

        </div>

    </div>

</form>

</body>

</html>
