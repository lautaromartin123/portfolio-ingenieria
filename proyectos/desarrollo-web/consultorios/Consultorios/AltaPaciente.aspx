<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AltaPaciente.aspx.cs" Inherits="Consultorios.AltaPaciente" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Alta Paciente</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>

    <form id="form1" runat="server" class="contenedor formulario">

        <div class="tarjeta">

            <h2>Alta de Paciente</h2>

            <asp:Label ID="LblNombre" runat="server" Text="Nombre:" CssClass="campo-label" />

            <asp:TextBox ID="TxtNombre" runat="server" CssClass="entrada" />

            <asp:Label ID="LblApellido" runat="server" Text="Apellido:" CssClass="campo-label" />

            <asp:TextBox ID="TxtApellido" runat="server" CssClass="entrada" />

            <asp:Label ID="LblTelefono" runat="server" Text="Teléfono:" CssClass="campo-label" />

            <asp:TextBox ID="TxtTelefono" runat="server" CssClass="entrada" />

            <asp:Label ID="LblTipoDocumento" runat="server" Text="Tipo de documento:" CssClass="campo-label" />

            <asp:DropDownList
                ID="DdTipoDocumento"
                runat="server"

                CssClass="entrada">
            </asp:DropDownList>

            <asp:Label ID="LblNumeroDocumento" runat="server" Text="Número de documento:" CssClass="campo-label" />

            <asp:TextBox
                ID="TxtNumeroDocumento"
                runat="server"

                CssClass="entrada">
            </asp:TextBox>

            <asp:Label ID="LblObraSocial" runat="server" Text="Obra social:" CssClass="campo-label" />

            <asp:DropDownList
                ID="DdObraSocial"
                runat="server"

                CssClass="entrada">
            </asp:DropDownList>

            <asp:Label ID="LblNumeroObraSocial" runat="server" Text="Número de afiliado:" CssClass="campo-label" />

            <asp:TextBox
                ID="TxtNumeroObraSocial"
                runat="server"

                CssClass="entrada">
            </asp:TextBox>

            <asp:Button
                ID="BtnGuardar"
                runat="server"
                Text="Guardar paciente"
                OnClick="BtnGuardar_Click"
                CssClass="boton" />

            <asp:Label
                ID="LblMensaje"
                runat="server"

                CssClass="mensaje">
            </asp:Label>

        </div>

    </form>

</body>
</html>
