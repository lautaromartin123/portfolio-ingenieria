<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="Registro.aspx.cs"
    Inherits="Consultorios.Registro" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Crear cuenta - Consultorios</title>

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
            CREAR CUENTA
        </h1>

        <p class="subtitulo">
            COMPLETÁ TUS DATOS
        </p>

        <div class="campo">

            <label class="campo-label">
                NOMBRE
            </label>

            <asp:TextBox
                ID="txtNombre"
                runat="server"
                CssClass="textbox"
                placeholder="Nombre">
            </asp:TextBox>

        </div>

        <div class="campo">

            <label class="campo-label">
                APELLIDO
            </label>

            <asp:TextBox
                ID="txtApellido"
                runat="server"
                CssClass="textbox"
                placeholder="Apellido">
            </asp:TextBox>

        </div>

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

        <div class="campo">

            <label class="campo-label">
                CONTRASEÑA
            </label>

            <div class="password-wrapper">

                <asp:TextBox
                    ID="txtPassword"
                    runat="server"
                    CssClass="textbox"
                    ClientIDMode="Static"
                    TextMode="Password">
                </asp:TextBox>

                <button
                    type="button"
                    class="btn-show"
                    onclick="toggleRegistroPassword('txtPassword', this)">
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
                    onclick="toggleRegistroPassword('txtConfirmarPassword', this)">
                    MOSTRAR
                </button>

            </div>

        </div>

        <asp:Button
            ID="btnRegistrar"
            runat="server"
            CssClass="btn-principal"
            Text="CREAR CUENTA"
            OnClick="BtnRegistrar_Click" />

        <asp:Label
            ID="lblMensaje"
            runat="server"

            CssClass="mensaje">
        </asp:Label>

        <div class="volver">

            ¿YA TENÉS UNA CUENTA?

            <asp:HyperLink
                ID="lnkLogin"
                runat="server"
                NavigateUrl="~/Login.aspx"
                Text=" INICIAR SESIÓN">
            </asp:HyperLink>

        </div>

    </div>

</form>

<script type="text/javascript">

    /**
     * @param {string} id
     * @param {HTMLButtonElement} boton
     */
    function toggleRegistroPassword(id, boton) {

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
