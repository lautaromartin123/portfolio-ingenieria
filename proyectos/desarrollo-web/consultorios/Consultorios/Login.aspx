<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="Login.aspx.cs"
    Inherits="Consultorios.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Iniciar sesión - Consultorios</title>

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
            INICIAR SESIÓN
        </h1>

        <p class="subtitulo">
            INGRESÁ CON TU GMAIL
        </p>

        <div class="campo">

            <label class="campo-label">
                GMAIL
            </label>

            <asp:TextBox
                ID="txtEmail"
                runat="server"
                CssClass="textbox"
                ClientIDMode="Static"
                TextMode="Email"
                placeholder="ejemplo@gmail.com">
            </asp:TextBox>

        </div>

        <div class="campo">

            <label class="campo-label">
                CONTRASEÑA
            </label>

            <div class="password-wrapper">

                <asp:TextBox
                    ID="txtPassword"
                    runat="server"
                    CssClass="textbox"
                    TextMode="Password"
                    ClientIDMode="Static">
                </asp:TextBox>

                <button
                    type="button"
                    class="btn-show"
                    onclick="toggleLoginPassword('txtPassword', this)">
                    MOSTRAR
                </button>

            </div>

        </div>

        <div class="forgot-row">

            <asp:HyperLink
                ID="lnkRecuperarPassword"
                runat="server"
                CssClass="forgot-link"
                NavigateUrl="~/RecuperarPassword.aspx"
                Text="¿OLVIDASTE TU CONTRASEÑA?">
            </asp:HyperLink>

        </div>

        <asp:Button
            ID="btnIngresar"
            runat="server"
            CssClass="btn-principal"
            Text="INGRESAR"
            OnClick="BtnIngresar_Click" />

        <asp:Label
            ID="lblMensaje"
            runat="server"

            CssClass="mensaje">
        </asp:Label>

        <div class="separador">
            O
        </div>

        <div class="registro">

            ¿NO TENÉS CUENTA?

            <asp:HyperLink
                ID="lnkRegistro"
                runat="server"
                NavigateUrl="~/Registro.aspx"
                Text=" CREAR UNA">
            </asp:HyperLink>

        </div>

    </div>

</form>

<script type="text/javascript">

    /**
     * @param {string} id
     * @param {HTMLButtonElement} boton
     */
    function toggleLoginPassword(id, boton) {

        var elemento = document.getElementById(id);

        if (!(elemento instanceof HTMLInputElement)) {
            return;
        }

        if (elemento.type === "password") {

            elemento.type = "text";
            boton.innerText = "OCULTAR";

        } else {

            elemento.type = "password";
            boton.innerText = "MOSTRAR";
        }
    }

</script>

</body>

</html>
