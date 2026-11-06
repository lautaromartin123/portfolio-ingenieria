<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="ResetPassword.aspx.cs"
    Inherits="Consultorios.ResetPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Nueva contraseña - Consultorios</title>

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
            NUEVA CONTRASEÑA
        </h1>

        <p class="subtitulo">
            Creá una contraseña nueva para volver a ingresar a tu cuenta.
        </p>

        <div class="campo">

            <label class="campo-label">
                NUEVA CONTRASEÑA
            </label>

            <div class="password-wrapper">

                <asp:TextBox
                    ID="txtNuevaPassword"
                    runat="server"
                    CssClass="textbox"
                    ClientIDMode="Static"
                    TextMode="Password">
                </asp:TextBox>

                <button
                    type="button"
                    class="btn-show"
                    onclick="togglePassword('txtNuevaPassword', this)">
                    MOSTRAR
                </button>

            </div>

        </div>

        <div class="campo">

            <label class="campo-label">
                CONFIRMAR CONTRASEÑA
            </label>

            <div class="password-wrapper">

                <asp:TextBox
                    ID="txtConfirmarPassword"
                    runat="server"
                    CssClass="textbox"
                    ClientIDMode="Static"
                    TextMode="Password">
                </asp:TextBox>

                <button
                    type="button"
                    class="btn-show"
                    onclick="togglePassword('txtConfirmarPassword', this)">
                    MOSTRAR
                </button>

            </div>

        </div>

        <div class="requisitos">
            La contraseña debe contener al menos 6 caracteres y ser diferente de la actual.
        </div>

        <asp:Button
            ID="btnCambiar"
            runat="server"
            CssClass="btn-principal"
            Text="CAMBIAR CONTRASEÑA"
            OnClick="BtnCambiar_Click" />

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

<script type="text/javascript">

function togglePassword(id, boton) {

    var input =
        document.getElementById(id);

    if (input.type === "password") {

        input.type = "text";
        boton.innerText = "OCULTAR";

    } else {

        input.type = "password";
        boton.innerText = "MOSTRAR";
    }
}

</script>

</body>

</html>
